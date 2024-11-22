using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RCBACONFERENCE.Models
{
    [Table("CONF_EvaluatorTable")]
    public class EvaluatorInfo
    {
        [Key] // Primary Key
        public string EvaluatorId { get; set; }

        [ForeignKey("UserConferenceRoles")] // FK to UserConferenceRoles
        [Required]
        public string UserRoleId { get; set; }

        [Required]
        [MaxLength(20)] // Limit the length of the status string
        public string Status { get; set; } = "Pending"; // Default to "Pending"
        // Navigation property for UserConferenceRoles
        public UserConferenceRoles UserConferenceRoles { get; set; }

        public ICollection<PaperAssignments> PaperAssignments { get; set; }
    }
}
