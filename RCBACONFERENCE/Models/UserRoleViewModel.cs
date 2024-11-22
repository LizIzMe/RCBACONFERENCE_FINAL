using System.ComponentModel.DataAnnotations;
namespace RCBACONFERENCE.Models
{
    public class UserRoleViewModel
    {
        [Required(ErrorMessage = "Please select a role")]
        public string SelectedRoleId { get; set; }

        [Required(ErrorMessage = "Please select a user")]
        public string SelectedUserId { get; set; }

        public List<ConferenceRoles> Roles { get; set; } = new List<ConferenceRoles>();
        public List<UsersConference> Users { get; set; } = new List<UsersConference>();
        public string? GeneratedUserRoleId { get; set; }

        public List<UserConferenceRoles> UserRoles { get; set; } = new List<UserConferenceRoles>();

    }
}