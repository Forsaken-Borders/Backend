using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using ForSakenBorders.Backend.Api.v1.Payloads;

namespace ForSakenBorders.Backend.Database
{
    /// <summary>
    /// A note for a <see cref="User"/>.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Empty constructor required by EFCore.
        /// </summary>
        private Note() { }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="notePayload">Retrieved from the API. See <see cref="NotePayload"/>.</param>
        /// <param name="owner">Who created the note.</param>
        /// <param name="sha512Generator">Used for creating the Content and Thumbnail hashes.</param>
        public Note(NotePayload notePayload, User owner, SHA512 sha512Generator)
        {
            Content = notePayload.Content.Split('\n').Aggregate((a, b) => a.Trim() + "\n" + b.Trim());
            ContentHash = sha512Generator.ComputeHash(Encoding.UTF8.GetBytes(notePayload.Content));
            Owner = owner;
            Tags = notePayload.Tags;
            Thumbnail = notePayload.Thumbnail;
            ThumbnailHash = sha512Generator.ComputeHash(notePayload.Thumbnail);
            Title = notePayload.Title.Trim();
        }

        /// <summary>
        /// The GUID of the note. Shouldn't ever change, and should be unique.
        /// </summary>
        [Key]
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Title of the note.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// Content of the note.
        /// </summary>
        [Required]
        [StringLength(10000)]
        public string Content { get; set; }

        /// <summary>
        /// The sha256 hash of the content. Should be verified before sending to the client. Should be recalculated on every content update.
        /// </summary>
        public byte[] ContentHash { get; set; }

        /// <summary>
        /// The user who owns the note. Notes can be transferred to other users.
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// A bunch of tags that can be used to categorize the note.
        /// </summary>
        [MaxLength(10)]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// The date the note was created.
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        /// <summary>
        /// The binary data of the note thumbnail.
        /// </summary>
        public byte[] Thumbnail { get; set; }

        /// <summary>
        /// The hash of the image data. Should be recalculated every time the thumbnail is updated.
        /// </summary>
        public byte[] ThumbnailHash { get; set; }

        /// <summary>
        /// Whether the note is private or public.
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}