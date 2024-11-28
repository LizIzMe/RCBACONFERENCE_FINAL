using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class SubmitEthicsCertificateViewModel
    {
        [Required]
        public string ResearchEventId { get; set; }

        public List<EventDropdown> ResearchEvents { get; set; } = new List<EventDropdown>();

        [Required]
        public string ResearchTitle { get; set; }

        [Required]
        public string Author { get; set; }

        public string? Authors { get; set; } = string.Empty;

        [Required]
        public IFormFile EthicsCertificate { get; set; }

        public List<EthicsCertificateStatus> StatusList { get; set; } = new List<EthicsCertificateStatus>();
    }

    public class EventDropdown
    {
        public string ResearchEventId { get; set; }
        public string DisplayText { get; set; }
    }

    public class EthicsCertificateStatus
    {
        public string EthicsID { get; set; }
        public string Author { get; set; }
        public string Authors { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
    }
}
