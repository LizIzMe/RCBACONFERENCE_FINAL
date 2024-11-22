using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class UploadReceiptViewModel
    {
        public string ResearchEventId { get; set; }

        public string? EventName { get; set; }

        public string? Classification { get; set; }

        public decimal Price { get; set; }

        [Required(ErrorMessage = "Input the reference number of your receipt")]
        public string ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Please upload your receipt.")]
        public IFormFile ReceiptFile { get; set; } 

        public string PaymentAccount { get; set; } = "Landbank Account: 1234-5678-9012"; 
    }
}
