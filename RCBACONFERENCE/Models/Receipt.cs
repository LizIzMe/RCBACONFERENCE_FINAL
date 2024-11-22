using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_Receipt")]
    public class Receipt
    {
        [Key]
        public string ReceiptId { get; set; }

        [Required]
        [ForeignKey("ResearchEvent")]
        public string ResearchEventId { get; set; }

        [Required]
        [ForeignKey("UsersConference")]
        public string UserId { get; set; }

        [Required]
        public byte[] ReceiptFile { get; set; }

        [Required]
        public string ReferenceNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public string? Comment { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public virtual ResearchEvent ResearchEvent { get; set; }
        public virtual UsersConference UsersConference { get; set; }
    
        public void GenerateReceiptId()
            {
                var year = DateTime.Now.Year.ToString();
                var randomDigits = new Random().Next(1000, 9999).ToString();
                ReceiptId = $"RECEIPT-{year}-{randomDigits}";
            }
    }
}