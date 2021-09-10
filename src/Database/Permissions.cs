using System;

namespace ForSakenBorders.Database
{
    /// <summary>
    /// Determines which actions a role can perform.
    /// </summary>
    [Flags]
    public enum Permissions : byte
    {
        /// <summary>
        /// Prevents all actions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows the user to view all objects related to their <see cref="User"/>.
        /// </summary>
        ViewOwn = 1,

        /// <summary>
        /// Allows the user to view all objects related to other <see cref="User"/>s.
        /// </summary>
        ViewAll = 2,

        /// <summary>
        /// Allows for the <see cref="User"/> to view general statistics.
        /// </summary>
        ViewStatistics = 4,

        /// <summary>
        /// Allows the user to create new objects.
        /// </summary>
        Create = 8,

        /// <summary>
        /// Allows the user to edit/delete their own objects.
        /// </summary>
        EditOwn = 16,

        /// <summary>
        /// Allows the user to edit (not delete) other's objects.
        /// </summary>
        EditAll = 32,

        /// <summary>
        /// Allows the user to delete other's objects.
        /// </summary>
        Delete = 64,

        /// <summary>
        /// Admin.
        /// </summary>
        All = ViewOwn | ViewAll | ViewStatistics | Create | EditOwn | EditAll | Delete
    }
}