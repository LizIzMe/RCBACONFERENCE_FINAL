using Microsoft.EntityFrameworkCore;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;

namespace RCBACONFERENCE.Services
{
    public class AssignmentPaper
    {
        private readonly ApplicationDbContext _context;

        public AssignmentPaper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AssignPapersToEvaluators(int maxAssignmentsPerEvaluator = 3)
        {
            // Assignment logic (from your original code)
            var unassignedPapers = await _context.UploadPapers
                .Include(up => up.PaperAssignments)
                .Where(up => !up.PaperAssignments.Any())
                .ToListAsync();

            var evaluators = await _context.EvaluatorInfo
                .Include(e => e.PaperAssignments)
                .Where(e => e.Status == "Accepted")
                .ToListAsync();

            if (!evaluators.Any())
            {
                throw new InvalidOperationException("No available evaluators found.");
            }

            var random = new Random();

            foreach (var paper in unassignedPapers)
            {
                var availableEvaluators = evaluators
                    .Where(e => e.PaperAssignments.Count < maxAssignmentsPerEvaluator)
                    .OrderBy(e => random.Next())
                    .ToList();

                if (!availableEvaluators.Any())
                {
                    throw new InvalidOperationException("No evaluators available for assignment.");
                }

                var assignedEvaluator = availableEvaluators.First();

                var assignment = new PaperAssignments
                {
                    UploadPaperID = paper.UploadPaperID,
                    EvaluatorId = assignedEvaluator.EvaluatorId
                };

                _context.PaperAssignments.Add(assignment);
            }

            await _context.SaveChangesAsync();
        }
    }

}
