using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_ResearchEvent")]
    public class ResearchEvent
    {
        [Key]
        public string ResearchEventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public string EventDescription { get; set; }

        [Required]
        public string EventLocation { get; set; }

        public byte[]? EventThumbnail { get; set; }

        [Required]
        public DateTime RegistrationOpen { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        public bool RequiresEthicsCertificate { get; set; } = false;

        public ICollection<ScheduleEvent> ScheduleEvents { get; set; } = new List<ScheduleEvent>();

        public void GenerateResearchEventId(int uniqueNumber)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            ResearchEventId = $"RE-{date}-{uniqueNumber:D4}";
        }
    }
}
