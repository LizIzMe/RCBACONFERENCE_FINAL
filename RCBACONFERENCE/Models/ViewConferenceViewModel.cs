namespace RCBACONFERENCE.Models
{
    public class ViewConferenceViewModel
    {
        public string ResearchEventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventLocation { get; set; }
        public byte[]? EventThumbnail { get; set; }
        public DateTime RegistrationOpen { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EndTime { get; set; }
        public List<ScheduleEventViewModel> Schedules { get; set; } = new List<ScheduleEventViewModel>();
        public bool HasUploadedReceipt { get; set; }
        public bool RequiresEthicsCertificate { get; set; }
        public string? Status { get; set; } 
        public string? Comment { get; set; }
    }
}
