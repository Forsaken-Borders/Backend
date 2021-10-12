using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForsakenBorders.Backend.Models
{
    /// <summary>
    /// Roles that are attached to a <see cref="User"/>. Determines which actions a <see cref="User"/> can perform, and shows status.
    /// </summary>
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
        /// If the role is official and shows off that it's recognized by the Forsaken Borders software.
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

        /// <summary>
        /// When the role was created at, in UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the role was updated at, in UTC time.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Role role
            && Id.Equals(role.Id)
            && Name == role.Name
            && Description == role.Description
            && IsOfficial == role.IsOfficial
            && EqualityComparer<byte[]>.Default.Equals(Icon, role.Icon)
            && Position == role.Position
            && UserPermissions == role.UserPermissions
            && NotePermissions == role.NotePermissions
            && CreatedAt == role.CreatedAt
            && UpdatedAt == role.UpdatedAt;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(IsOfficial);
            hash.Add(Icon);
            hash.Add(Position);
            hash.Add(UserPermissions);
            hash.Add(NotePermissions);
            hash.Add(CreatedAt);
            hash.Add(UpdatedAt);
            return hash.ToHashCode();
        }
    }
}