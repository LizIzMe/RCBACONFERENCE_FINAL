using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_UserConferenceRoles")]
    public class UserConferenceRoles
    {
        [Key]
        public string UserRoleId { get; set; } 

        [ForeignKey("ConferenceRoles")]
        public string RoleId { get; set; } 

        [ForeignKey("UsersConference")]
        public string UserId { get; set; } 

        public virtual ConferenceRoles ConferenceRoles { get; set; }
        public virtual UsersConference UsersConference { get; set; }

        public static string GenerateUserRoleId(string roleName)
        {
            var random = new Random();
            int randomNumber = random.Next(1000, 9999); 
            return $"{roleName.ToUpper()}-{randomNumber}"; 
        }
    }
}
