using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class VerifyViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Verification Code")]
        public string VerificationCode { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }

    }
}