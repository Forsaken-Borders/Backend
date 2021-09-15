using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ForSakenBorders.Backend.Api.v1.Payloads;
using ForSakenBorders.Backend.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForSakenBorders.Backend.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SHA512 _sha512Generator;
        private readonly BackendContext _database;

        public UsersController(BackendContext forSakenBordersContext, SHA512 sha512Generator)
        {
            _database = forSakenBordersContext;
            _sha512Generator = sha512Generator;
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
            else if (!userPayload.ValidateEmail())
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

            User newUser = new(userPayload, _sha512Generator);
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
            else if (!userPayload.ValidateEmail())
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
            requestedUser.PasswordHash = _sha512Generator.ComputeHash(Encoding.UTF8.GetBytes(userPayload.Password));
            requestedUser.Username = userPayload.Username;
            requestedUser.Roles = userPayload.Roles ?? new();
            requestedUser.FirstName = userPayload.FirstName;
            requestedUser.LastName = userPayload.LastName;
            await _database.SaveChangesAsync();
            return Ok();
        }
    }
}