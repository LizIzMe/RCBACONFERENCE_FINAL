using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_ConferenceRoles")]
    public class ConferenceRoles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public string RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }

        public static string GenerateRoleId()
        {
            var random = new Random();
            int randomNumber = random.Next(1000, 9999);
            var roleId = $"ROLE-{randomNumber}";
            System.Diagnostics.Debug.WriteLine($"Generated Role ID: {roleId}");
            return roleId;
        }
    }
}
