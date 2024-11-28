using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_UploadPapers")]
    public class UploadPaperInfo
    {
        [Key]
        public string UploadPaperID { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("UserConferenceRoles")]
        public string UserRoleId { get; set; }

        [ForeignKey("UsersConference")]
        [Required]
        public string UserId { get; set; }

        [ForeignKey("ResearchEvent")] 
        [Required]
        public string ResearchEventId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [MaxLength(250)]
        public string Abstract { get; set; }

        public string Affiliation { get; set; }

        public string? Authors { get; set; } // Nullable Authors field

        public string Keywords { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        [Required]
        public string FileName { get; set; }

        public string FileType { get; set; }

        [ForeignKey("EthicsCertificate")]
        public string? EthicsID { get; set; } 

        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Default status is "Pending"
        public UserConferenceRoles UserConferenceRoles { get; set; }
        public UsersConference UsersConference{ get; set; }
        public ResearchEvent ResearchEvent { get; set; }
        public EthicsCertificate EthicsCertificate { get; set; } 
        public ICollection<PaperAssignments> PaperAssignments { get; set; }
    }
}
