using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ForSakenBorders.Backend.Api.v1.Payloads;
using ForSakenBorders.Backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForSakenBorders.Backend.Api.v1
{
    [ApiController]
    [Route("/api/v1/users")]
    public class Users : ControllerBase
    {
        private readonly SHA512 _sha512Generator;
        private readonly BackendContext _database;

        public Users(BackendContext forSakenBordersContext, SHA512 sha512Generator)
        {
            _database = forSakenBordersContext;
            _sha512Generator = sha512Generator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            string authorizationToken = Request.Headers["Authorization"];
            authorizationToken ??= Guid.Empty.ToString();
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }

            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == id);
            if (requestedUser == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(requestedUser);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(UserPayload userPayload)
        {
            string authorizationToken = Request.Headers["Authorization"];
            authorizationToken ??= Guid.Empty.ToString();
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is not null && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.Create)))
            {
                return Unauthorized("You do not have permission to create accounts.");
            }
            else if (userPayload is null)
            {
                return BadRequest("User payload is null.");
            }
            else if (!userPayload.ValidateEmail())
            {
                return BadRequest("Email is invalid.");
            }
            else if (_database.Users.Any(databaseUser => databaseUser.Email == userPayload.Email))
            {
                return BadRequest("Email is already in use.");
            }
            else
            {
                User newUser = new(userPayload, _sha512Generator);
                _database.Users.Add(newUser);
                await _database.SaveChangesAsync();
                return Created($"/api/v1/users/{newUser.Id}", newUser);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            string authorizationToken = Request.Headers["Authorization"];
            authorizationToken ??= Guid.Empty.ToString();
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }

            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == id);
            if (requestedUser is null)
            {
                return NotFound();
            }
            else if (requestedUser.Id != user.Id && !user.Roles.Any(role => role.UserPermissions.HasFlag(Permissions.Delete)))
            {
                return Unauthorized("You do not have permission to delete this account.");
            }
            else
            {
                requestedUser.IsDeleted = true;
                requestedUser.FirstName = "Deleted User";
                requestedUser.LastName = "#" + requestedUser.Id;
                requestedUser.Username = "Deleted User #" + requestedUser.Id;
                requestedUser.PasswordHash = null;
                requestedUser.Token = Guid.Empty;
                requestedUser.TokenExpiration = DateTime.UtcNow;
                requestedUser.UpdatedAt = DateTime.UtcNow;
                await _database.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Patch(UserPayload userPayload)
        {
            string authorizationToken = Request.Headers["Authorization"];
            authorizationToken ??= Guid.Empty.ToString();
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }
            else if (userPayload is null)
            {
                return BadRequest("User Payload is null.");
            }

            User requestedUser = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Id == userPayload.Id);
            if (requestedUser is null)
            {
                return NotFound();
            }
            else if (requestedUser.Id != user.Id && !user.Roles.Any(role => role.UserPermissions.HasFlag(Permissions.EditAll)))
            {
                return Unauthorized("You do not have permission to update this account.");
            }
            else if (!userPayload.ValidateEmail())
            {
                return BadRequest("Email is invalid.");
            }
            else
            {
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
}