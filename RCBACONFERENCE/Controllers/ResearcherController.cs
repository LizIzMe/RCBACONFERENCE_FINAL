using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;
using RCBACONFERENCE.Services;
using System.Security.Claims;
using System.Text.Json;

namespace RCBACONFERENCE.Controllers
{
    [Authorize] // Add authorization to ensure user is logged in
    public class ResearcherController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ResearcherController> _logger;
        private readonly AssignmentPaper _assignmentPaper;
        public ResearcherController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ResearcherController> logger,
            AssignmentPaper assignmentPaper)
            : base(context) 
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _assignmentPaper = assignmentPaper;
        }

        //Upload Paper View
        [HttpGet]
        public IActionResult UploadPaper()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to upload a paper.";
                return RedirectToAction("Login", "Account");
            }

            var researchEvents = _context.Registration
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    ResearchEventId = r.ResearchEvent.ResearchEventId,
                    DisplayText = $"{r.ResearchEvent.ResearchEventId} : {r.ResearchEvent.EventName}"
                })
                .ToList();

            var approvedEthics = _context.EthicsCertificate
                .Where(ec => ec.UserId == userId && ec.Status == "Approved")
                .Select(ec => new
                {
                    ec.EthicsID,
                    ec.ResearchEventId,
                    ec.ResearchTitle
                })
                .ToList();

            ViewBag.ResearchEvents = researchEvents;
            ViewBag.ApprovedEthics = approvedEthics;

            return View(new UploadPaperInfo()); 
        }


        //Upload paper logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPaper(UploadPaperInfo model, IFormFile file, string[] Authors)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to upload a paper.";
                    return RedirectToAction("Login", "Account");
                }

                var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Login", "Account");
                }

                var researchEvent = await _context.ResearchEvent.FirstOrDefaultAsync(re => re.ResearchEventId == model.ResearchEventId);
                if (researchEvent == null)
                {
                    TempData["Error"] = "Research event not found.";
                    PopulateResearchEventsViewBag(userId);
                    return View(model);
                }

                if (researchEvent.RequiresEthicsCertificate &&
                    (user.Affiliation.Equals("Polytechnic University of the Philippines", StringComparison.OrdinalIgnoreCase) ||
                     user.Affiliation.Equals("PUP", StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.IsNullOrEmpty(model.EthicsID))
                    {
                        TempData["Error"] = "You must select an approved Ethics Certificate for this event.";
                        PopulateResearchEventsViewBag(userId);
                        return View(model);
                    }

                    var ethicsCertificate = await _context.EthicsCertificate
                        .FirstOrDefaultAsync(ec => ec.EthicsID == model.EthicsID && ec.UserId == userId && ec.ResearchEventId == researchEvent.ResearchEventId);

                    if (ethicsCertificate == null || ethicsCertificate.Status != "Approved")
                    {
                        TempData["Error"] = "The selected Ethics Certificate is invalid or not approved.";
                        PopulateResearchEventsViewBag(userId);
                        return View(model);
                    }
                }

                var userRole = await _context.UserConferenceRoles.FirstOrDefaultAsync(r => r.UserId == userId);
                if (userRole == null)
                {
                    TempData["Error"] = "User role not found.";
                    PopulateResearchEventsViewBag(userId);
                    return View(model);
                }

                if (file == null || file.Length == 0)
                {
                    TempData["Error"] = "Please select a valid file.";
                    PopulateResearchEventsViewBag(userId);
                    return View(model);
                }

                var allowedExtensions = new[] { ".pdf" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only PDF files are allowed.";
                    PopulateResearchEventsViewBag(userId);
                    return View(model);
                }

                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var paperInfo = new UploadPaperInfo
                {
                    UserId = userId,
                    UserRoleId = userRole.UserRoleId,
                    ResearchEventId = model.ResearchEventId,
                    Title = model.Title,
                    Abstract = model.Abstract,
                    Author = $"{user.FirstName} {user.LastName}",
                    Authors = Authors.Length > 0 ? string.Join(", ", Authors) : "Unknown Author",
                    Affiliation = user.Affiliation,
                    Keywords = model.Keywords,
                    FileData = fileData,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    Status = "Pending",
                    EthicsID = model.EthicsID,
                    CreatedAt = DateTime.Now
                };

                _context.UploadPapers.Add(paperInfo);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Paper uploaded successfully!";

                try
                {
                    await _assignmentPaper.AssignPapersToEvaluators();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Paper uploaded but assignment failed: {ex.Message}");
                }

                return RedirectToAction("ViewPaper");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the paper.");
                TempData["Error"] = "An error occurred while uploading the paper. Please try again.";
                PopulateResearchEventsViewBag(User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View(model);
            }
        }

        private void PopulateResearchEventsViewBag(string userId)
        {
            var researchEvents = _context.Registration
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    ResearchEventId = r.ResearchEvent.ResearchEventId,
                    DisplayText = $"{r.ResearchEvent.ResearchEventId} : {r.ResearchEvent.EventName}"
                })
                .ToList();

            ViewBag.ResearchEvents = researchEvents;
        }

        [HttpGet]
        public IActionResult GetApprovedEthics(string researchEventId)
        {
            if (string.IsNullOrEmpty(researchEventId))
            {
                _logger.LogWarning("ResearchEventId is null or empty.");
                return Json(new { message = "Invalid Research Event ID." });
            }

            var approvedEthics = _context.EthicsCertificate
                .Where(ec => ec.ResearchEventId == researchEventId && ec.Status == "Approved")
                .Select(ec => new
                {
                    ec.EthicsID,
                    ec.ResearchTitle
                })
                .ToList();

            _logger.LogInformation("Approved Ethics for Event {EventId}: {Ethics}", researchEventId, approvedEthics);

            return Json(approvedEthics);
        }

        public IActionResult HomeResearcher()
        {
            return View();
        }

        //For view uploaded document
        [HttpGet]
        public async Task<IActionResult> ViewDocument(string id)
        {

                try
                {
                    // Fetch the paper by its ID
                    var paper = await _context.UploadPapers.FirstOrDefaultAsync(p => p.UploadPaperID == id);
                    if (paper == null)
                    {
                        TempData["Error"] = "Document not found.";
                        return RedirectToAction("ViewPaper");
                    }

                    // Return the PDF file as a FileStreamResult
                    return File(paper.FileData, paper.FileType);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while retrieving the document.";
                    return RedirectToAction("ViewPaper");
                }
            
        }

        //For view uploaded table paper list
        [HttpGet]
        public async Task<IActionResult> ViewPaper()
        {
            try
            {
                // Get logged-in user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to view papers.";
                    return RedirectToAction("Login", "Account");
                }

                // Fetch papers uploaded by the user
                var papers = await _context.UploadPapers
                    .Include(p => p.ResearchEvent)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                return View(papers); // Pass the list to the view
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching the uploaded papers.";
                return RedirectToAction("UploadPaper");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewInvitation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to view invitations.";
                return RedirectToAction("Login", "Account");
            }

            var invitations = await _context.EvaluatorInfo
                .Include(e => e.UserConferenceRoles)
                .ThenInclude(ucr => ucr.ConferenceRoles)
                .Where(e => e.UserConferenceRoles.UserId == userId)
                .ToListAsync();

            return View(invitations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvitation(string evaluatorId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID

                // Fetch the evaluator invitation
                var evaluator = await _context.EvaluatorInfo
                    .Include(e => e.UserConferenceRoles)
                    .FirstOrDefaultAsync(e => e.EvaluatorId == evaluatorId && e.UserConferenceRoles.UserId == userId);

                if (evaluator == null)
                {
                    TempData["ErrorMessage"] = "Invitation not found or you are not authorized to access it.";
                    return RedirectToAction("ViewInvitation");
                }

                evaluator.Status = "Accepted";

                // Assign the "Official Evaluator" role
                var officialEvaluatorRole = await _context.ConferenceRoles.FirstOrDefaultAsync(r => r.RoleName == "Evaluator");
                if (officialEvaluatorRole == null)
                {
                    TempData["ErrorMessage"] = "Role 'Official Evaluator' not found. Please contact the administrator.";
                    return RedirectToAction("ViewInvitation");
                }

                var newUserRole = new UserConferenceRoles
                {
                    UserRoleId = UserConferenceRoles.GenerateUserRoleId("Evaluator"),
                    RoleId = officialEvaluatorRole.RoleId,
                    UserId = userId
                };

                _context.UserConferenceRoles.Add(newUserRole);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have accepted the invitation and are now an official evaluator.";
                return RedirectToAction("ViewInvitation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                TempData["ErrorMessage"] = "An error occurred while accepting the invitation.";
                return RedirectToAction("ViewInvitation");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInvitation(string evaluatorId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID

                // Fetch the evaluator invitation
                var evaluator = await _context.EvaluatorInfo
                    .Include(e => e.UserConferenceRoles)
                    .FirstOrDefaultAsync(e => e.EvaluatorId == evaluatorId && e.UserConferenceRoles.UserId == userId);

                if (evaluator == null)
                {
                    TempData["ErrorMessage"] = "Invitation not found or you are not authorized to access it.";
                    return RedirectToAction("ViewInvitation");
                }

                evaluator.Status = "Rejected";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have rejected the invitation.";
                return RedirectToAction("ViewInvitation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting invitation");
                TempData["ErrorMessage"] = "An error occurred while rejecting the invitation.";
                return RedirectToAction("ViewInvitation");
            }
        }

        [HttpGet]
        public IActionResult ViewConference()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var researchEvents = _context.ResearchEvent
                .Include(re => re.ScheduleEvents) 
                .ToList();

            var viewModel = researchEvents.Select(re => new ViewConferenceViewModel
            {
                ResearchEventId = re.ResearchEventId,
                EventName = re.EventName,
                EventDescription = re.EventDescription,
                EventLocation = re.EventLocation,
                EventThumbnail = re.EventThumbnail,
                RegistrationOpen = re.RegistrationOpen,
                RegistrationDeadline = re.RegistrationDeadline,
                RequiresEthicsCertificate = re.RequiresEthicsCertificate,
                Schedules = re.ScheduleEvents.Select(se => new ScheduleEventViewModel
                {
                    EventDate = se.EventDate,
                    EndTime = se.EndTime
                }).ToList(),
                HasUploadedReceipt = _context.Receipt.Any(r => r.ResearchEventId == re.ResearchEventId && r.UserId == userId),
                Status = _context.Receipt
                    .Where(r => r.ResearchEventId == re.ResearchEventId && r.UserId == userId)
                    .Select(r => r.Status)
                    .FirstOrDefault(),
                Comment = _context.Receipt
                    .Where(r => r.ResearchEventId == re.ResearchEventId && r.UserId == userId)
                    .Select(r => r.Comment)
                    .FirstOrDefault()
            }).ToList();

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult UploadReceipt(string eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            var researchEvent = _context.ResearchEvent.FirstOrDefault(e => e.ResearchEventId == eventId);

            if (researchEvent == null || user == null)
            {
                return NotFound();
            }

            decimal price = user.Classification == "Student" ? 6000 : 7500;

            var model = new UploadReceiptViewModel
            {
                ResearchEventId = eventId,
                EventName = researchEvent.EventName,
                Classification = user.Classification,
                Price = price,
                PaymentAccount = "Landbank Account: 1234-5678-9012"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadReceipt(UploadReceiptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); 
                }

                TempData["ErrorMessage"] = "There was an error uploading your receipt. Please check the details and try again.";
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var receipt = new Receipt
                {
                    ResearchEventId = model.ResearchEventId,
                    UserId = userId,
                    ReferenceNumber = model.ReferenceNumber,
                    Status = "Pending",
                    UploadedDate = DateTime.Now
                };

                using (var memoryStream = new MemoryStream())
                {
                    model.ReceiptFile.CopyTo(memoryStream);
                    receipt.ReceiptFile = memoryStream.ToArray();
                }

                receipt.GenerateReceiptId();

                _context.Receipt.Add(receipt);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Receipt uploaded successfully. Awaiting approval.";
                return RedirectToAction("ViewConference", "Researcher");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); 
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadNewReceipt(string researchEventId, string referenceNumber, IFormFile receiptFile)
        {
            if (receiptFile == null || receiptFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please upload a valid receipt file.";
                return RedirectToAction("ViewConference");
            }

            var receipt = _context.Receipt.FirstOrDefault(r => r.ResearchEventId == researchEventId && r.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (receipt == null)
            {
                TempData["ErrorMessage"] = "Receipt not found.";
                return RedirectToAction("ViewConference");
            }

            using (var memoryStream = new MemoryStream())
            {
                receiptFile.CopyTo(memoryStream);
                receipt.ReceiptFile = memoryStream.ToArray();
            }

            receipt.ReferenceNumber = referenceNumber;
            receipt.Status = "Pending";
            receipt.Comment = null; 
            receipt.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "New receipt uploaded successfully!";
            return RedirectToAction("ViewConference");
        }

        [HttpGet]
        public IActionResult SubmitEthicsCertificate()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to submit an ethics certificate.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            var researchEvents = _context.Registration
                .Where(r => r.UserId == userId && r.ResearchEvent.RequiresEthicsCertificate)
                .Select(r => new EventDropdown
                {
                    ResearchEventId = r.ResearchEvent.ResearchEventId,
                    DisplayText = $"{r.ResearchEvent.ResearchEventId} : {r.ResearchEvent.EventName}"
                })
                .ToList();

            var ethicsCertificates = _context.EthicsCertificate
                .Where(ec => ec.UserId == userId)
                .Select(ec => new EthicsCertificateStatus
                {
                    EthicsID = ec.EthicsID,
                    Author = ec.Author,
                    Authors = ec.Authors,
                    EventName = ec.ResearchEvent.EventName,
                    Status = ec.Status
                })
                .ToList();

            var viewModel = new SubmitEthicsCertificateViewModel
            {
                ResearchEvents = researchEvents,
                Author = $"{user.FirstName} {user.LastName}",
                StatusList = ethicsCertificates
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEthicsCertificate(SubmitEthicsCertificateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid form submission.";
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            var researchEvent = _context.ResearchEvent.FirstOrDefault(re => re.ResearchEventId == model.ResearchEventId);
            if (researchEvent == null)
            {
                TempData["ErrorMessage"] = "Research event not found.";
                return View(model);
            }

            var registration = _context.Registration.FirstOrDefault(r =>
                r.UserId == userId && r.ResearchEventId == model.ResearchEventId);
            if (registration == null)
            {
                TempData["ErrorMessage"] = "You are not registered for this event.";
                return View(model);
            }

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await model.EthicsCertificate.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var ethicsCertificate = new EthicsCertificate
            {
                UserId = userId,
                ResearchEventId = model.ResearchEventId,
                ResearchTitle = model.ResearchTitle,
                Author = model.Author,
                Authors = model.Authors != null && model.Authors.Any() ? string.Join(", ", model.Authors) : null,
                EthicsCertficate = fileData
            };

            _context.EthicsCertificate.Add(ethicsCertificate);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ethics Certificate submitted successfully!";
            return RedirectToAction("SubmitEthicsCertificate");
        }

        [HttpGet]
        public async Task<IActionResult> ViewEthicsCertificate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Ethics Certificate ID is required.";
                return RedirectToAction("SubmitEthicsCertificate");
            }

            var ethicsCertificate = await _context.EthicsCertificate
                .FirstOrDefaultAsync(ec => ec.EthicsID == id);

            if (ethicsCertificate == null)
            {
                TempData["ErrorMessage"] = "Ethics Certificate not found.";
                return RedirectToAction("SubmitEthicsCertificate");
            }

            return File(ethicsCertificate.EthicsCertficate, "application/pdf");
        }

        [HttpGet]
        public IActionResult ViewEthicsCertificatePage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            var ethicsCertificates = _context.EthicsCertificate
                .Where(e => e.UserId == userId)
                .Select(e => new ViewEthicsCertificateViewModel
                {
                    EthicsID = e.EthicsID,
                    Author = e.Author,
                    Authors = e.Authors,
                    EventName = e.ResearchEvent.EventName,
                    ResearchTitle = e.ResearchTitle,
                    Status = e.Status,
                    Comment = e.Comment
                })
                .ToList();

            var viewModel = new ViewEthicsCertificateViewModel
            {
                StatusList = ethicsCertificates
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResubmitEthicsCertificate(string EthicsID, string ResearchTitle, string Authors, IFormFile EthicsCertificate)
        {
            var ethicsCertificate = _context.EthicsCertificate.FirstOrDefault(ec => ec.EthicsID == EthicsID);

            if (ethicsCertificate == null)
            {
                TempData["ErrorMessage"] = "Ethics Certificate not found.";
                return RedirectToAction("ViewEthicsCertificatePage");
            }

            ethicsCertificate.ResearchTitle = ResearchTitle;
            ethicsCertificate.Authors = Authors;
            ethicsCertificate.UpdatedOn = DateTime.Now;

            if (EthicsCertificate != null && EthicsCertificate.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await EthicsCertificate.CopyToAsync(memoryStream);
                    ethicsCertificate.EthicsCertficate = memoryStream.ToArray();
                }
            }

            ethicsCertificate.Status = "Pending";
            ethicsCertificate.Comment = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ethics Certificate resubmitted successfully!";
            return RedirectToAction("ViewEthicsCertificatePage");
        }

    }
}
