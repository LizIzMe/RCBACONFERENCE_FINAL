namespace RCBACONFERENCE.Models
{
    public class CheckReceiptsViewModel
    {
        public List<ResearchEventViewModel> ResearchEvents { get; set; } = new List<ResearchEventViewModel>();
        public List<ReceiptViewModel> Receipts { get; set; } = new List<ReceiptViewModel>();
        public string? SelectedResearchEventId { get; set; }
    }

}
