using System;
using System.Linq;
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
    [Authorize("UserTokenHandler")]
    [ApiController]
    [Route("/api/v1/notes")]
    public class Notes : ControllerBase
    {
        private readonly SHA512 _sha512Generator;
        private readonly BackendContext _database;

        public Notes(BackendContext forSakenBordersContext, SHA512 sha512Generator)
        {
            _database = forSakenBordersContext;
            _sha512Generator = sha512Generator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            string authorizationToken = Request.Headers["Authorization"];
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }

            Note note = await _database.Notes.FirstOrDefaultAsync(databaseNote => databaseNote.Id == id);
            if (note is null)
            {
                return NotFound();
            }
            else if (note.Owner.Token != authorization && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.ViewAll)))
            {
                return Unauthorized("Not the owner of the note and missing the \"ViewAll\" permission.");
            }

            return _sha512Generator.ComputeHash(Encoding.UTF8.GetBytes(note.Content)) != note.ContentHash && Request.Headers["Expect"] != "100-continue"
                ? StatusCode(500, "The note's content does not rehash to the note's hash. This means the content is highly likely to have been modified without the use of the API, and it could be dangerous to open up the note. To retrieve the contents regardless, pass the \"Expect: 100-continue\" header.")
                : Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> Post(NotePayload notePayload)
        {
            string authorizationToken = Request.Headers["Authorization"];
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }
            else if (!user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.Create)))
            {
                return Unauthorized("You do not have permission to create notes.");
            }
            else if (notePayload is null)
            {
                return BadRequest("Note payload is null.");
            }
            else
            {
                Note note = new(notePayload, user, _sha512Generator);
                _database.Notes.Add(note);
                await _database.SaveChangesAsync();
                return Created($"/api/v1/notes/{note.Id}", note);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            string authorizationToken = Request.Headers["Authorization"];
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }

            Note note = await _database.Notes.FirstOrDefaultAsync(databaseNote => databaseNote.Id == id);
            if (note is null)
            {
                return NotFound();
            }

            // Case 1: The user is not the owner of the note and does not have permission to delete
            // Case 2: The user is the owner of the note and has permission to edit it.
            if ((user.Id != note.Owner.Id && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.Delete))) || (user.Id == note.Owner.Id && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.EditOwn))))
            {
                return Unauthorized("You do not have permission to delete this note.");
            }
            else
            {
                _database.Notes.Remove(note);
                await _database.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Patch(NotePayload newNote)
        {
            string authorizationToken = Request.Headers["Authorization"];
            Guid authorization = Guid.Parse(authorizationToken);
            User user = await _database.Users.FirstOrDefaultAsync(databaseUser => databaseUser.Token == authorization);
            if (user is null)
            {
                return Unauthorized("Invalid authorization token.");
            }
            else if (newNote is null)
            {
                return BadRequest("Note is null.");
            }
            else if (newNote.Id == Guid.Empty)
            {
                return BadRequest("Note ID is empty.");
            }

            Note note = await _database.Notes.FirstOrDefaultAsync(databaseNote => databaseNote.Id == newNote.Id);
            if (note is null)
            {
                return NotFound();
            }
            else if ((user.Id != note.Owner.Id && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.EditAll))) || (user.Id == note.Owner.Id && !user.Roles.Any(role => role.NotePermissions.HasFlag(Permissions.EditOwn))))
            {
                return Unauthorized("You do not have permission to edit this note.");
            }
            else
            {
                note.Content = newNote.Content;
                note.ContentHash = _sha512Generator.ComputeHash(Encoding.UTF8.GetBytes(note.Content));
                note.Title = newNote.Title;
                note.Tags = newNote.Tags;
                note.IsPrivate = newNote.IsPrivate;
                await _database.SaveChangesAsync();
                return Ok();
            }
        }
    }
}