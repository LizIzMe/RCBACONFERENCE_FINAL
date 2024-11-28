using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_EthicsCertificate")]
    public class EthicsCertificate
    {
        [Key]
        public string EthicsID { get; set; }

        [Required]
        [ForeignKey("UsersConference")]
        public string UserId { get; set; } 

        [Required]
        [ForeignKey("ResearchEvent")]
        public string ResearchEventId { get; set; }

        [Required]
        public string ResearchTitle {  get; set; }

        [Required]
        public string Author { get; set; } 

        public string? Authors { get; set; } 

        [Required]
        public byte[] EthicsCertficate { get; set; } 

        [Required]
        public string Status { get; set; } = "Pending";

        public string? Comment { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; } = DateTime.Now; 

        public DateTime? UpdatedOn { get; set; } 

        public UsersConference UsersConference { get; set; }
        public ResearchEvent ResearchEvent { get; set; }

        public EthicsCertificate()
        {
            EthicsID = GenerateEthicsId();
        }

        private static string GenerateEthicsId()
        {
            string year = DateTime.Now.Year.ToString();
            string randomDigits4 = new Random().Next(1000, 9999).ToString();
            string randomDigits2 = new Random().Next(10, 99).ToString();
            return $"ETHICS-{year}-{randomDigits4}-{randomDigits2}";
        }
    }
}
