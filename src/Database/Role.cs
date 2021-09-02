using System.ComponentModel.DataAnnotations;

namespace Kiki.Database
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Permissions UserPermissions { get; set; } = Permissions.EditOwn | Permissions.ViewOwn;
        public Permissions NotePermissions { get; set; } = Permissions.EditOwn | Permissions.ViewOwn;
    }
}