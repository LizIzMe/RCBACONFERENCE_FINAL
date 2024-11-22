using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_EvaluationTable")]
    public class Evaluation
    {
        [Key]
        public string EvaluationId { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("UploadPaperInfo")]
        public string UploadPaperID { get; set; }

        [ForeignKey("EvaluatorInfo")]
        public string EvaluatorId { get; set; }

        public string Comments { get; set; }

        // Ratings for different categories
        public int ScientificNovelty { get; set; }
        public int SignificanceContribution { get; set; }
        public int TechnicalQuality { get; set; }
        public int DepthResearch { get; set; }
        public int ClarityPresentation { get; set; }
        public int RelevanceTheme { get; set; }
        public int OriginalityApproach { get; set; }

        public DateTime EvaluatedAt { get; set; }

        // Navigation Properties
        public UploadPaperInfo UploadPaperInfo { get; set; }
        public EvaluatorInfo EvaluatorInfo { get; set; }
    }
}
