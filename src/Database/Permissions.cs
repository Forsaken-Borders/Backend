using System;

namespace Kiki.Database
{
    [Flags]
    public enum Permissions : byte
    {
        None,
        ViewOwn,
        ViewAll,
        Create,
        EditOwn,
        EditAll,
        Delete,
        All = ViewOwn | ViewAll | Create | EditOwn | EditAll | Delete
    }
}