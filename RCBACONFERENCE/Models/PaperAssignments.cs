using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace RCBACONFERENCE.Models
{
    [Table("CONF_PaperAssignmentTable")]
    public class PaperAssignments
    {
        [Key]
        public string AssignmentId { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("UploadPaperInfo")]
        public string UploadPaperID { get; set; }

        [ForeignKey("EvaluatorInfo")]
        public string EvaluatorId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public UploadPaperInfo UploadPaperInfo { get; set; }
        public EvaluatorInfo EvaluatorInfo { get; set; }
    }

}
