namespace RCBACONFERENCE.Models
{
    public class ReceiptViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Classification { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public string? Comment { get; set; }
        public byte[] ReceiptFile { get; set; } 
        public string ResearchEventId { get; set; }
    }
}
