using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCBACONFERENCE.Models
{
    [Table("CONF_UserConference")]
    public class UsersConference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Affiliation { get; set; }     
        
        [Required]
        public string Classification { get; set; }

        [Required]
        public string CountryRegion { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string VerificationCode { get; set; }

        [Required]
        public bool IsVerified { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public UsersConference()
        {
            UserId = GenerateUserId();
            VerificationCode = GenerateVerificationCode();
        }

        private static string GenerateUserId()
        {
            string year = DateTime.Now.Year.ToString();
            string randomDigits4 = new Random().Next(1000, 9999).ToString();
            string randomDigits2 = new Random().Next(10, 99).ToString();
            return $"{year}-RCBACONFERENCE-{randomDigits4}-{randomDigits2}";
        }

        public string GenerateVerificationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            char[] code = new char[4];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            return new string(code);
        }
        public ICollection<UploadPaperInfo> UploadPapers { get; set; } // Navigation property
    }
}
