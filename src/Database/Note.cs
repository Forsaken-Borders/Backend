using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kiki.Database
{
    /// <summary>
    /// A note for a <see cref="Database.User"/>.
    /// </summary>
    public class Note
    {
        [Key]
        public int Id { get; }
        /// <summary>
        /// Title of the note.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Content of the note.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The sha256 hash of the content. Should be verified before sending to the client. Should be recalculated on every content update.
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// TODO: Replace with an author type or id. Can optionally be transferred to other users.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A bunch of tags that can be used to categorize the note.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// The date the note was created.
        /// </summary>
        public DateTime Date { get; } = DateTime.UtcNow;

        /// <summary>
        /// The binary data of the note thumbnail.
        /// </summary>
        public byte[] ThumbnailContents { get; set; }

        /// <summary>
        /// The hash of the image data. Should be recalculated every time the thumbnail is updated.
        /// </summary>
        public string ThumbnailHash { get; set; }

        /// <summary>
        /// The private key used to encrypt/decrypt the note.
        /// </summary>
        public string PrivateKey { get; }
    }
}