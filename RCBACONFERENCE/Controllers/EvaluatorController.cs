using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Security.Claims;
using Org.BouncyCastle.Crypto.Macs;

namespace RCBACONFERENCE.Controllers
{
    [Authorize]
    public class EvaluatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EvaluatorController> _logger;

        public EvaluatorController(ApplicationDbContext context, ILogger<EvaluatorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult HomeEvaluator()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PaperTable()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to view assigned papers.";
                    return RedirectToAction("Login", "Account");
                }

                var evaluator = await _context.EvaluatorInfo
                    .Include(e => e.UserConferenceRoles)
                    .FirstOrDefaultAsync(e => e.UserConferenceRoles.UserId == userId);

                if (evaluator == null)
                {
                    TempData["Error"] = "Evaluator profile not found.";
                    //return RedirectToAction("HomeEvaluator");
                }

                var assignedPapers = await _context.PaperAssignments
                    .Where(pa => pa.EvaluatorId == evaluator.EvaluatorId)
                    .Include(pa => pa.UploadPaperInfo)
                    .Select(pa => pa.UploadPaperInfo)
                    .ToListAsync();

                return View(assignedPapers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retrieving assigned papers.");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("HomeEvaluator");
            }
        }


        // Display evaluation form for a specific paper
        [HttpGet]
        public async Task<IActionResult> EvaluatePaper(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "Invalid paper ID.";
                    return RedirectToAction("PaperTable");
                }

                // Fetch the paper by ID
                var paper = await _context.UploadPapers.FirstOrDefaultAsync(p => p.UploadPaperID == id);

                if (paper == null)
                {
                    TempData["ErrorMessage"] = "Paper not found.";
                    return RedirectToAction("PaperTable");
                }

                // Pass the paper to the evaluation view
                return View(paper);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the evaluation form.";
                return RedirectToAction("PaperTable");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEvaluation(
        string UploadPaperID,
        string Comments,
        int ScientificNovelty,
        int SignificanceContribution,
        int TechnicalQuality,
        int DepthResearch,
        int ClarityPresentation,
        int RelevanceTheme,
        int OriginalityApproach)
        {
            try
            {
                if (string.IsNullOrEmpty(UploadPaperID))
                {
                    TempData["ErrorMessage"] = "Invalid paper ID.";
                    return RedirectToAction("PaperTable");
                }

                // Fetch the paper
                var paper = await _context.UploadPapers
                    .Include(p => p.UsersConference) // Include researcher info
                    .FirstOrDefaultAsync(p => p.UploadPaperID == UploadPaperID);
                if (paper == null)
                {
                    TempData["ErrorMessage"] = "Paper not found.";
                    return RedirectToAction("PaperTable");
                }

                // Get the evaluator's EvaluatorId based on the logged-in user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var evaluator = await _context.EvaluatorInfo
                    .Include(e => e.UserConferenceRoles)
                    .FirstOrDefaultAsync(e => e.UserConferenceRoles.UserId == userId);

                if (evaluator == null)
                {
                    TempData["ErrorMessage"] = "Evaluator profile not found.";
                    return RedirectToAction("PaperTable");
                }

                // Save the evaluation
                var evaluation = new Evaluation
                {
                    EvaluationId = Guid.NewGuid().ToString(),
                    UploadPaperID = UploadPaperID,
                    EvaluatorId = evaluator.EvaluatorId, // Use EvaluatorId from the database
                    Comments = Comments,
                    ScientificNovelty = ScientificNovelty,
                    SignificanceContribution = SignificanceContribution,
                    TechnicalQuality = TechnicalQuality,
                    DepthResearch = DepthResearch,
                    ClarityPresentation = ClarityPresentation,
                    RelevanceTheme = RelevanceTheme,
                    OriginalityApproach = OriginalityApproach,
                    EvaluatedAt = DateTime.UtcNow
                };

                _context.Evaluation.Add(evaluation);
                await _context.SaveChangesAsync();

                // Calculate the average score for the paper
                var evaluations = await _context.Evaluation
                    .Where(e => e.UploadPaperID == UploadPaperID)
                    .ToListAsync();

                var averageScore = evaluations
                    .Select(e => new[]
                    {
                        e.ScientificNovelty,
                        e.SignificanceContribution,
                        e.TechnicalQuality,
                        e.DepthResearch,
                        e.ClarityPresentation,
                        e.RelevanceTheme,
                        e.OriginalityApproach
                            }.Average())
                            .Average();

                // Automatically accept or reject the paper
                string paperStatus;
                if (averageScore < 2.5)
                {
                    paper.Status = "Rejected";
                    paperStatus = "Rejected";
                }
                else
                {
                    paper.Status = "Accepted";
                    paperStatus = "Accepted";
                }

                _context.UploadPapers.Update(paper);
                await _context.SaveChangesAsync();

                // Notify researcher via email
                var emailSubject = "Paper Evaluation Notification";
                var emailBody = $@"
            Dear {paper.UsersConference.FirstName} {paper.UsersConference.LastName},

            Your paper titled '{paper.Title}' has been evaluated. Below is the summary:

            Average Score: {averageScore:F2}
            Status: {paperStatus}

            Comments: {Comments}

            Thank you for your submission.

            Best regards,
            RCBACONFERENCE Team";

                await SendEmailAsync(paper.UsersConference.Email, emailSubject, emailBody);

                TempData["SuccessMessage"] = "Evaluation submitted successfully, and the researcher has been notified.";
                return RedirectToAction("PaperTable");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the evaluation.";
                _logger.LogError(ex, "Error submitting evaluation.");
                return RedirectToAction("PaperTable");
            }
        }

        private async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("RCBACONFERENCE Notification", "rmocrdlnotification@gmail.com"));
                message.To.Add(new MimeKit.MailboxAddress(recipientEmail, recipientEmail));
                message.Subject = subject;

                message.Body = new MimeKit.TextPart("plain")
                {
                    Text = body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("rmocrdlnotification@gmail.com", "uhflpdedbetywhxu");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email sent successfully to {recipientEmail}");
            }
            catch (Exception ex)
            {

            }
        }
    }
}