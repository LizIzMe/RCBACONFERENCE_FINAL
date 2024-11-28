using System.Collections.Generic;

namespace RCBACONFERENCE.Models
{
    public class CheckEthicsViewModel
    {
        public string SelectedResearchEventId { get; set; } 
        public List<EventDropdown> ResearchEvents { get; set; } = new List<EventDropdown>(); 
        public List<EthicsCertificateDetails> EthicsCertificates { get; set; } = new List<EthicsCertificateDetails>();
    }

    public class EthicsCertificateDetails
    {
        public string EthicsID { get; set; }
        public string ResearchTitle { get; set; }
        public string Author { get; set; }
        public string CoAuthors { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}
