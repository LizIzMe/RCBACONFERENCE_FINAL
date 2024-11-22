using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class ProfileSettingsViewModel
    {
        public string UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Affiliation")]
        public string Affiliation { get; set; }

        [Required]
        [Display(Name = "Country/Region")]
        public string CountryRegion { get; set; }
    }
}
