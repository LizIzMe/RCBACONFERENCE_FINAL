using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_ScheduleEvent")]
    public class ScheduleEvent
    {
        [Key]
        public string ScheduleEventId { get; set; }

        [ForeignKey("ResearchEvent")]
        public string ResearchEventId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public virtual ResearchEvent ResearchEvent { get; set; }

        public void GenerateScheduleEventId(int index)
        {
            var date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            ScheduleEventId = $"SE-{date}-{index:D3}";
        }
    }
}
