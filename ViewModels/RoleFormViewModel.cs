using System.ComponentModel.DataAnnotations;

namespace UserManagement_WithIdentity.ViewModels
{
    public class RoleFormViewModel
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
    }
}
