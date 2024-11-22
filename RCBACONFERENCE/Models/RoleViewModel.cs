using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class RoleViewModel
    {
        public string? RoleId { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        [StringLength(50, ErrorMessage = "Role Name cannot be longer than 50 characters")]
        public string RoleName { get; set; }
        public List<ConferenceRoles> Roles { get; set; } = new List<ConferenceRoles>();

    }
}
