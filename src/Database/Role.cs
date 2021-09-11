using System;
using System.ComponentModel.DataAnnotations;

namespace ForSakenBorders.Backend.Database
{
    /// <summary>
    /// Roles that are attached to a <see cref="User"/>. Determines which actions a <see cref="User"/> can perform, and shows status.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Empty constructor required by EFCore.
        /// </summary>
        private Role() { }

        /// <summary>
        /// The GUID of the role. Shouldn't ever change, and should be unique.
        /// </summary>
        [Key]
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The name of the role. Can be duplicated.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A short description of the role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If the role is official and shows off that it's recognized by the ForSaken Borders software.
        /// </summary>
        public bool IsOfficial { get; set; }

        /// <summary>
        /// The role icon.
        /// </summary>
        public byte[] Icon { get; set; }

        /// <summary>
        /// Determines which position the role should take in the role list.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Determines what actions those with the role can perform on users.
        /// </summary>
        public Permissions UserPermissions { get; set; } = Permissions.EditOwn | Permissions.ViewOwn;

        /// <summary>
        /// Determines what actions those with the role can perform on notes.
        /// </summary>
        public Permissions NotePermissions { get; set; } = Permissions.EditOwn | Permissions.ViewOwn;
    }
}