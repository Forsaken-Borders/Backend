using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForSakenBorders.Backend.Api.v1.Payloads
{
    /// <summary>
    /// The payload the user sends to create or edit a new note.
    /// </summary>
    public class NotePayload
    {
        /// <summary>
        /// Title of the note.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }

        /// <summary>
        /// Content of the note.
        /// </summary>
        [Required]
        [StringLength(10000, MinimumLength = 3)]
        public string Content { get; set; }

        /// <summary>
        /// A bunch of tags that can be used to categorize the note.
        /// </summary>
        [MaxLength(10)]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// The binary data of the note thumbnail.
        /// </summary>
        public byte[] Thumbnail { get; set; }

        /// <summary>
        /// Optional parameter. Should only be used when editing a specific note.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Whether the note is private or public.
        /// </summary>
        public bool IsPrivate { get; set; } = true;
    }
}