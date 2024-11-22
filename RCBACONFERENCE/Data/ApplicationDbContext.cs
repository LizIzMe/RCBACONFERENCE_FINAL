using RCBACONFERENCE.Models;
using Microsoft.EntityFrameworkCore;


namespace RCBACONFERENCE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<UsersConference> UsersConference { get; set; } // UsersConference Table
        public DbSet<ConferenceRoles> ConferenceRoles { get; set; } // ConferenceRoles Table
        public DbSet<UserConferenceRoles> UserConferenceRoles { get; set; } // UserConferenceRoles Table
        public DbSet<ResearchEvent> ResearchEvent { get; set; } // ResearchEvents Table
        public DbSet<ScheduleEvent> ScheduleEvent { get; set; } // ScheduleEvent Table
        public DbSet<EvaluatorInfo> EvaluatorInfo { get; set; } // EvaluatorInfo Table
        public DbSet<UploadPaperInfo> UploadPapers { get; set; } //UploadedPapers Table
        public DbSet<PaperAssignments> PaperAssignments { get; set; } //PaperAssignments Table
        public DbSet<Evaluation> Evaluation { get; set; } //Evaluation Table
        public DbSet<Receipt> Receipt { get; set; } //Receipt Table
        public DbSet<Registration> Registration { get; set; } //Registration Table

    }
}
