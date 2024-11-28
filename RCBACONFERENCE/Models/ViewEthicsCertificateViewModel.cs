using System.Collections.Generic;

namespace RCBACONFERENCE.Models
{
    public class ViewEthicsCertificateViewModel
    {
        public string EthicsID { get; set; } 
        public string Author { get; set; } 
        public string Authors { get; set; } 
        public string EventName { get; set; } 
        public string Status { get; set; } 
        public string ResearchTitle { get; set; }
        public string Comment { get; set; }

        public List<ViewEthicsCertificateViewModel> StatusList { get; set; } = new List<ViewEthicsCertificateViewModel>();
    }
}
