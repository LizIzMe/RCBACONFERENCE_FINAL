using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_Registration")]
    public class Registration
    {
        [Key]
        public string RegistrationId { get; set; }

        [Required]
        [ForeignKey("UsersConference")]
        public string UserId { get; set; }

        [Required]
        [ForeignKey("ResearchEvent")]
        public string ResearchEventId { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public virtual UsersConference UsersConference { get; set; }
        public virtual ResearchEvent ResearchEvent { get; set; }

        public void GenerateRegistrationId()
        {
            var year = DateTime.Now.Year.ToString();
            var randomDigits4 = new Random().Next(1000, 9999).ToString();
            var randomDigits2 = new Random().Next(10, 99).ToString();
            RegistrationId = $"REGISTRATION-{year}-{randomDigits4}-{randomDigits2}";
        }
    }
}
