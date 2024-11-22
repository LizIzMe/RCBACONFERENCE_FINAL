namespace RCBACONFERENCE.Models
{
    public class InvitedEvaluatorViewModel
    {
        public string UserId { get; set; } // For all researchers
        public string UserRoleId { get; set; } // For invited researchers
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsSelected { get; set; } // Checkbox for selection
        public string Status { get; set; } // Pending, Rejected, Accepted
    }
}
