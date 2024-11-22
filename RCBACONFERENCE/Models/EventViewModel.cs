using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class EventViewModel
    {
        [Required(ErrorMessage = "Event Name is required")]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Event Description is required")]
        public string EventDescription { get; set; }

        [Required(ErrorMessage = "Event Location is required")]
        public string EventLocation { get; set; }

        public byte[]? EventThumbnail { get; set; }

        [Required(ErrorMessage = "Registration Open date is required")]
        [DataType(DataType.Date)]
        public DateTime RegistrationOpen { get; set; }

        [Required(ErrorMessage = "Registration Deadline date is required")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDeadline { get; set; }

        public List<ScheduleEventViewModel> Schedules { get; set; } = new List<ScheduleEventViewModel>();
    }

    public class ScheduleEventViewModel
    {
        [Required(ErrorMessage = "Event Date is required")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "End Time is required")]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }
    }
}
