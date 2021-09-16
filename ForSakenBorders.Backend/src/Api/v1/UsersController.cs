using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ForSakenBorders.Backend.Api.v1.Payloads;
using ForSakenBorders.Backend.Database;
using ForSakenBorders.Backend.Utilities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ForSakenBorders.Backend.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly BackendContext _database;
        private readonly IConfiguration _configuration;

        public UsersController(BackendContext forSakenBordersContext, IConfiguration configuration)
        {
            _database = forSakenBordersContext;
            _configuration = configuration;
        }

        [HttpGet("{requestedUserId}")]
        public async Task<IActionResult> Get(Guid requestedUserId)
        {
            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == requestedUserId);
            if (requestedUser is null)
            {
                return NotFound();
            }
            else if (requestedUser.IsDeleted)
            {
                return StatusCode(410, requestedUser);
            }
            else
            {
                return Ok(requestedUser);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(UserPayload userPayload)
        {
            // TODO: Apparently MediatR can handle this.
            if (userPayload is null)
            {
                return BadRequest("User payload is null.");
            }
            else if (!userPayload.Email.ValidateEmail())
            {
                return BadRequest("email is invalid.");
            }
            else if (userPayload.FirstName?.Trim().Length > 32)
            {
                return BadRequest("first_name length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.LastName?.Trim().Length > 32)
            {
                return BadRequest("last_name length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.Username?.Trim().Length > 32)
            {
                return BadRequest("username length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.Username?.Trim().Length < 3)
            {
                return BadRequest("username length should be greater than or equal to 3 characters in length.");
            }
            else if (_database.Users.Any(databaseUser => databaseUser.Email == userPayload.Email))
            {
                return Conflict("email is already in use.");
            }

            User newUser = new(userPayload);
            _database.Users.Add(newUser);
            await _database.SaveChangesAsync();
            return Created($"/api/v1/users/{newUser.Id}", newUser.Token);
        }

        [HttpDelete("{requestedUserId}")]
        public async Task<IActionResult> Delete(Guid requestedUserId)
        {
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == Guid.Parse(Request.Headers["Authorization"]));
            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == requestedUserId);
            if (requestedUser is null)
            {
                return NotFound();
            }
            else if (user.Id != requestedUser.Id && !user.Roles.Any(role => role.UserPermissions.HasFlag(Permissions.Delete)))
            {
                return Unauthorized("Missing permissions.");
            }
            else
            {
                requestedUser.IsDeleted = true;
                requestedUser.PasswordHash = null;
                requestedUser.Token = Guid.Empty;
                requestedUser.TokenExpiration = DateTime.UtcNow;
                requestedUser.UpdatedAt = DateTime.UtcNow;
                await _database.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPut("{requestedUserId}")]
        public async Task<IActionResult> Put(Guid requestedUserId, UserPayload userPayload)
        {
            if (userPayload is null)
            {
                return BadRequest("User Payload is null.");
            }
            else if (requestedUserId == Guid.Empty)
            {
                return BadRequest("Id should not be null.");
            }

            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == Guid.Parse(Request.Headers["Authorization"]));
            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == requestedUserId);
            // TODO: Apparently MediatR can handle this.
            if (requestedUser is null)
            {
                return NotFound();
            }
            else if (!userPayload.Email.ValidateEmail())
            {
                return BadRequest("email is invalid.");
            }
            else if (userPayload.FirstName.Trim().Length > 32)
            {
                return BadRequest("first_name length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.LastName.Trim().Length > 32)
            {
                return BadRequest("last_name length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.Username.Trim().Length > 32)
            {
                return BadRequest("username length should be less than or equal to 32 characters in length.");
            }
            else if (userPayload.Username.Trim().Length < 3)
            {
                return BadRequest("username length should be greater than or equal to 3 characters in length.");
            }
            else if (_database.Users.Any(databaseUser => databaseUser.Email == userPayload.Email && databaseUser.Id != requestedUser.Id))
            {
                return Conflict("email is already in use.");
            }
            else if (user.Id != requestedUserId && !user.Roles.Any(role => role.UserPermissions.HasFlag(Permissions.EditAll)))
            {
                return Unauthorized("Missing permissions.");
            }
            else if (userPayload.Equals(requestedUser))
            {
                return StatusCode(304);
            }

            requestedUser.Email = userPayload.Email;
            requestedUser.PasswordSalt = RandomNumberGenerator.GetBytes(1024);
            requestedUser.PasswordHash = userPayload.Password.Argon2idHash(requestedUser.PasswordSalt);
            requestedUser.Username = userPayload.Username;
            requestedUser.Roles = userPayload.Roles ?? new();
            requestedUser.FirstName = userPayload.FirstName;
            requestedUser.LastName = userPayload.LastName;
            await _database.SaveChangesAsync();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginPayload loginPayload)
        {
            if (loginPayload is null)
            {
                return BadRequest("Login payload is null.");
            }
            else if (!loginPayload.Email.ValidateEmail())
            {
                return BadRequest("email is invalid.");
            }
            else if (loginPayload.TokenExpiration != DateTime.MinValue && loginPayload.TokenExpiration < DateTime.UtcNow)
            {
                return BadRequest("token_expiration is in the past.");
            }

            User user = _database.Users.FirstOrDefault(databaseUser => databaseUser.Email == loginPayload.Email);
            if (user is null)
            {
                return NotFound("Unknown email.");
            }
            else if (!loginPayload.Password.Argon2idHash(user.PasswordSalt).SequenceEqual(user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }
            else if (user.IsDeleted)
            {
                return StatusCode(410, "User is deleted.");
            }

            user.Token = Guid.NewGuid();
            user.TokenExpiration = loginPayload.TokenExpiration;
            await _database.SaveChangesAsync();
            return Ok(user.Token);
        }

        [AllowAnonymous]
        [HttpPost("forgot")]
        public IActionResult Post(string emailAddress)
        {
            if (emailAddress is null)
            {
                return BadRequest("Email address is null.");
            }
            else if (!emailAddress.ValidateEmail())
            {
                return BadRequest("Email address is invalid.");
            }

            User user = _database.Users.FirstOrDefault(databaseUser => databaseUser.Email == emailAddress);
            if (user == null)
            {
                return NotFound("Unknown email.");
            }
            else if (user.IsDeleted)
            {
                return StatusCode(410, "User is deleted. Please contact support if you wish to restore your account.");
            }
            user.RecoveryToken = Guid.NewGuid();
            user.RecoveryTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            _database.SaveChanges();

            SmtpClient client = new();
            client.Connect(_configuration["email:host"], _configuration.GetValue<int>("email:port"), SecureSocketOptions.StartTlsWhenAvailable);
            client.Authenticate(_configuration["email:noreply_address"], _configuration["email:password"]);

            MimeMessage message = new(emailAddress, "Reset your password", $@"
                <p>
                    Hello,
                </p>
                <p>
                    We recently recieved a request to reset your password. Click the link below to reset your password.
                    <a href=""https://{_configuration["domain_name"]}/users/reset/{user.RecoveryToken}"">
                        Reset your password
                    </a>
                </p>
                <p>
                    If you did not request a password reset, please ignore this email.
                </p>
                <p>
                    Thanks,<br>
                    {_configuration["email:noreply_name"]}
                </p>
            ");
            client.Send(message);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset/{recoveryToken}")]
        public IActionResult Post(Guid recoveryToken, string password)
        {
            if (recoveryToken == Guid.Empty)
            {
                return BadRequest("Recovery token is null.");
            }
            else if (password is null)
            {
                return BadRequest("Password is null.");
            }
            else if (password.Length < 8)
            {
                return BadRequest("Password must be more than 8 characters.");
            }
            else if (password.Length > 72)
            {
                return BadRequest("Password must be less than 72 characters.");
            }

            User user = _database.Users.FirstOrDefault(databaseUser => databaseUser.RecoveryToken == recoveryToken);
            if (user is null)
            {
                return NotFound("Unknown recovery token.");
            }
            else if (user.RecoveryTokenExpiration < DateTime.UtcNow)
            {
                return BadRequest("Recovery token has expired.");
            }
            else if (user.IsDeleted)
            {
                return StatusCode(410, "User is deleted. Please contact support if you wish to restore your account.");
            }

            user.PasswordSalt = RandomNumberGenerator.GetBytes(1024);
            user.PasswordHash = password.Argon2idHash(user.PasswordSalt);
            user.RecoveryToken = Guid.Empty;
            user.RecoveryTokenExpiration = DateTime.MinValue;
            _database.SaveChanges();

            return StatusCode(204, "Password reset.");
        }
    }
}