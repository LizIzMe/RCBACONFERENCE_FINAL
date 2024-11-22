using System.ComponentModel.DataAnnotations;

namespace RCBACONFERENCE.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name (Optional)")]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Affiliation { get; set; }
        
        [Required]
        public string Classification { get; set; }

        [Required]
        [Display(Name = "Country/Region")]
        public string CountryRegion { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string Password { get; set; }
    }
}