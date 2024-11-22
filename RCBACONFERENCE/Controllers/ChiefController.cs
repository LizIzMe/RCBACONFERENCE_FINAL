using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using RCBACONFERENCE.Services;

namespace RCBACONFERENCE.Controllers
{
    public class ChiefController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChiefController> _logger;
        private readonly ReportService _reportService;


        public ChiefController(ApplicationDbContext context, ILogger<ChiefController> logger, ReportService reportService)
        {
            _context = context;
            _logger = logger;
            _reportService = reportService;

        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            var model = new RoleViewModel
            {
                Roles = _context.ConferenceRoles.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            try
            {
                ModelState.Remove("RoleId");

                if (ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting to create role with name: {model.RoleName}");

                    var existingRole = await _context.ConferenceRoles
                        .FirstOrDefaultAsync(r => r.RoleName.ToLower() == model.RoleName.ToLower());

                    if (existingRole != null)
                    {
                        ModelState.AddModelError("RoleName", "This role name already exists.");
                        model.Roles = await _context.ConferenceRoles.ToListAsync();
                        return View(model);
                    }

                    var newRole = new ConferenceRoles
                    {
                        RoleId = ConferenceRoles.GenerateRoleId(),
                        RoleName = model.RoleName
                    };

                    await _context.ConferenceRoles.AddAsync(newRole);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Role created successfully!";
                    return RedirectToAction(nameof(CreateRole));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", $"Error creating role: {ex.Message}");
            }

            model.Roles = await _context.ConferenceRoles.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateUserRole()
        {
            _logger.LogInformation("Loading CreateUserRole GET action");

            var roles = _context.ConferenceRoles.ToList();
            var users = _context.UsersConference.ToList();

            _logger.LogInformation($"Loaded {roles.Count} roles and {users.Count} users");

            var userRoles = _context.UserConferenceRoles
                .Include(ur => ur.UsersConference)
                .Include(ur => ur.ConferenceRoles)
                .ToList();

            var model = new UserRoleViewModel
            {
                Roles = roles,
                Users = users,
                UserRoles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserRole(UserRoleViewModel model)
        {
            _logger.LogInformation("POST CreateUserRole started");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid");
                    model.Roles = await _context.ConferenceRoles.ToListAsync();
                    model.Users = await _context.UsersConference.ToListAsync();
                    return View(model);
                }

                var selectedRole = await _context.ConferenceRoles
                    .FirstOrDefaultAsync(r => r.RoleId == model.SelectedRoleId);

                if (selectedRole == null)
                {
                    ModelState.AddModelError("", "Selected role not found");
                    model.Roles = await _context.ConferenceRoles.ToListAsync();
                    model.Users = await _context.UsersConference.ToListAsync();
                    return View(model);
                }

                var selectedUser = await _context.UsersConference
                    .FirstOrDefaultAsync(u => u.UserId == model.SelectedUserId);

                if (selectedUser == null)
                {
                    ModelState.AddModelError("", "Selected user not found");
                    model.Roles = await _context.ConferenceRoles.ToListAsync();
                    model.Users = await _context.UsersConference.ToListAsync();
                    return View(model);
                }

                var existingUserRole = await _context.UserConferenceRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == model.SelectedUserId
                                          && ur.RoleId == model.SelectedRoleId);

                if (existingUserRole != null)
                {
                    ModelState.AddModelError("", "This user already has this role assigned");
                    model.Roles = await _context.ConferenceRoles.ToListAsync();
                    model.Users = await _context.UsersConference.ToListAsync();
                    return View(model);
                }

                string generatedUserRoleId = UserConferenceRoles.GenerateUserRoleId(selectedRole.RoleName);

                var userRole = new UserConferenceRoles
                {
                    UserRoleId = generatedUserRoleId,
                    RoleId = model.SelectedRoleId,
                    UserId = model.SelectedUserId
                };

                await _context.UserConferenceRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User Role ID generated: {generatedUserRoleId} and saved successfully.";
                return RedirectToAction(nameof(CreateUserRole));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user role");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.Roles = await _context.ConferenceRoles.ToListAsync();
                model.Users = await _context.UsersConference.ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserRole(string userRoleId)
        {
            var userRole = await _context.UserConferenceRoles.FindAsync(userRoleId);

            if (userRole == null)
            {
                TempData["ErrorMessage"] = "User role not found.";
                return RedirectToAction(nameof(CreateUserRole));
            }

            _context.UserConferenceRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User role deleted successfully.";
            return RedirectToAction(nameof(CreateUserRole));
        }

        public IActionResult HomeChief()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ManageConferenceEvents()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEvent(EventViewModel model, IFormFile? EventThumbnail)
        {

            if (model.RegistrationOpen > model.RegistrationDeadline)
            {
                TempData["ErrorMessage"] = "The registration open date cannot be later than the registration deadline.";
                return View("~/Views/Chief/ManageConferenceEvents.cshtml", model);
            }

            foreach (var schedule in model.Schedules)
            {
                if (schedule.EventDate < model.RegistrationOpen)
                {
                    TempData["ErrorMessage"] = "Event dates cannot occur before the registration open date.";
                    return View("~/Views/Chief/ManageConferenceEvents.cshtml", model);
                }
            }

            if (ModelState.IsValid)
            {
                var researchEvent = new ResearchEvent();
                var uniqueNumber = _context.ResearchEvent.Count() + 1;
                researchEvent.GenerateResearchEventId(uniqueNumber);

                researchEvent.EventName = model.EventName;
                researchEvent.EventDescription = model.EventDescription;
                researchEvent.EventLocation = model.EventLocation;

                if (EventThumbnail != null && EventThumbnail.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        EventThumbnail.CopyTo(memoryStream);
                        researchEvent.EventThumbnail = memoryStream.ToArray();
                    }
                }

                researchEvent.RegistrationOpen = model.RegistrationOpen;
                researchEvent.RegistrationDeadline = model.RegistrationDeadline;

                for (int i = 0; i < model.Schedules.Count; i++)
                {
                    var schedule = model.Schedules[i];
                    var scheduleEvent = new ScheduleEvent
                    {
                        EventDate = schedule.EventDate,
                        EndTime = schedule.EndTime,
                        ResearchEventId = researchEvent.ResearchEventId
                    };
                    scheduleEvent.GenerateScheduleEventId(i + 1);
                    researchEvent.ScheduleEvents.Add(scheduleEvent);
                }

                _context.ResearchEvent.Add(researchEvent);
                _context.SaveChanges();

                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("ManageConferenceEvents");
            }

            TempData["ErrorMessage"] = "Failed to create event. Please check the details and try again.";
            return View("~/Views/Chief/ManageConferenceEvents.cshtml", model);
        }

        [HttpGet]
        public IActionResult UpdateEvents()
        {
            var researchEvents = _context.ResearchEvent
                .Include(e => e.ScheduleEvents)
                .ToList();

            var viewModel = researchEvents.Select(re => new ViewConferenceViewModel
            {
                ResearchEventId = re.ResearchEventId,
                EventName = re.EventName,
                EventDescription = re.EventDescription,
                EventLocation = re.EventLocation,
                RegistrationOpen = re.RegistrationOpen,
                RegistrationDeadline = re.RegistrationDeadline,
                EventThumbnail = re.EventThumbnail,
                Schedules = re.ScheduleEvents.Select(se => new ScheduleEventViewModel
                {
                    EventDate = se.EventDate,
                    EndTime = se.EndTime
                }).ToList()
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateEvent(string ResearchEventId, DateTime RegistrationOpen, DateTime RegistrationDeadline, List<ScheduleEventViewModel> Schedules)
        {
            var researchEvent = _context.ResearchEvent
                .Include(e => e.ScheduleEvents)
                .FirstOrDefault(e => e.ResearchEventId == ResearchEventId);

            if (researchEvent == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("UpdateEvents");
            }

            if (RegistrationOpen > RegistrationDeadline)
            {
                TempData["ErrorMessage"] = "Registration Open date cannot be later than Registration Deadline.";
                return RedirectToAction("UpdateEvents");
            }

            foreach (var schedule in Schedules)
            {
                if (schedule.EventDate < RegistrationOpen)
                {
                    TempData["ErrorMessage"] = "Event dates cannot occur before the Registration Open date.";
                    return RedirectToAction("UpdateEvents");
                }
            }

            researchEvent.RegistrationOpen = RegistrationOpen;
            researchEvent.RegistrationDeadline = RegistrationDeadline;

            for (int i = 0; i < Schedules.Count; i++)
            {
                var schedule = researchEvent.ScheduleEvents.ElementAtOrDefault(i);
                if (schedule != null)
                {
                    schedule.EventDate = Schedules[i].EventDate;
                    schedule.EndTime = Schedules[i].EndTime;
                }
                else
                {
                    var newSchedule = new ScheduleEvent
                    {
                        ScheduleEventId = Guid.NewGuid().ToString(),
                        ResearchEventId = ResearchEventId,
                        EventDate = Schedules[i].EventDate,
                        EndTime = Schedules[i].EndTime
                    };
                    researchEvent.ScheduleEvents.Add(newSchedule);
                }
            }

            // Save changes
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Event updated successfully!";
            return RedirectToAction("UpdateEvents");
        }

        [HttpGet]
        public IActionResult CheckReceipts(string? researchEventId)
        {
            var researchEvents = _context.ResearchEvent
                .Select(e => new ResearchEventViewModel
                {
                    ResearchEventId = e.ResearchEventId,
                    DisplayText = $"{e.ResearchEventId} : {e.EventName}"
                })
                .ToList();

            var receiptsQuery = _context.Receipt
                .Include(r => r.UsersConference)
                .Include(r => r.ResearchEvent)
                .AsQueryable();

            if (!string.IsNullOrEmpty(researchEventId))
            {
                receiptsQuery = receiptsQuery.Where(r => r.ResearchEventId == researchEventId);
            }

            var receipts = receiptsQuery
                .Select(r => new ReceiptViewModel
                {
                    UserId = r.UserId,
                    Name = $"{r.UsersConference.FirstName} {r.UsersConference.LastName}",
                    Classification = r.UsersConference.Classification,
                    ReferenceNumber = r.ReferenceNumber,
                    Status = r.Status,
                    Comment = r.Comment,
                    ReceiptFile = r.ReceiptFile,
                    ResearchEventId = r.ResearchEventId
                })
                .ToList();

            var model = new CheckReceiptsViewModel
            {
                ResearchEvents = researchEvents,
                Receipts = receipts,
                SelectedResearchEventId = researchEventId
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewReceipt(string userId, string researchEventId)
        {
            var receipt = _context.Receipt
                .Include(r => r.UsersConference)
                .FirstOrDefault(r => r.UserId == userId && r.ResearchEventId == researchEventId);

            if (receipt == null)
            {
                return NotFound("Receipt not found.");
            }

            var mimeType = "image/png";

            return File(receipt.ReceiptFile, mimeType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReceiptStatus(string userId, string researchEventId, string status, string? comment)
        {
            var receipt = _context.Receipt
                .FirstOrDefault(r => r.UserId == userId && r.ResearchEventId == researchEventId);

            if (receipt == null)
            {
                TempData["ErrorMessage"] = $"Receipt for User ID {userId} not found.";
                return RedirectToAction("CheckReceipts");
            }

            if (receipt.Status == "Approved" && status != "Approved")
            {
                TempData["ErrorMessage"] = "An approved receipt cannot be changed to another status.";
                return RedirectToAction("CheckReceipts");
            }

            receipt.Status = status;

            if (status == "Rejected")
            {
                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["ErrorMessage"] = $"Comment is required for rejecting the receipt for User ID {userId}.";
                    return RedirectToAction("CheckReceipts");
                }
                receipt.Comment = comment;
            }
            else if (status == "Approved")
            {
                var existingRegistration = _context.Registration
                    .FirstOrDefault(r => r.UserId == userId && r.ResearchEventId == researchEventId);

                if (existingRegistration == null)
                {
                    var registration = new Registration
                    {
                        UserId = userId,
                        ResearchEventId = researchEventId,
                        RegistrationDate = DateTime.Now
                    };
                    registration.GenerateRegistrationId();

                    _context.Registration.Add(registration);
                }
            }

            if (status != "Rejected")
            {
                receipt.Comment = null;
            }

            receipt.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Receipt status updated successfully!";
            return RedirectToAction("CheckReceipts");
        }

        //ViewInvitations
        public IActionResult ViewInvitedEvaluators()
        {
            var researchers = _context.UserConferenceRoles
                .Include(ucr => ucr.UsersConference)
                .Include(ucr => ucr.ConferenceRoles)
                .Where(ucr => ucr.ConferenceRoles.RoleName == "Researcher")
                .Select(ucr => new InvitedEvaluatorViewModel
                {
                    UserId = ucr.UsersConference.UserId,
                    FullName = $"{ucr.UsersConference.FirstName} {ucr.UsersConference.LastName}",
                    Email = ucr.UsersConference.Email,
                    IsSelected = false
                })
                .ToList();

            var invitedResearchers = _context.EvaluatorInfo
                .Include(e => e.UserConferenceRoles)
                    .ThenInclude(ucr => ucr.UsersConference)
                .Select(e => new InvitedEvaluatorViewModel
                {
                    UserRoleId = e.UserRoleId,
                    FullName = $"{e.UserConferenceRoles.UsersConference.FirstName} {e.UserConferenceRoles.UsersConference.LastName}",
                    Email = e.UserConferenceRoles.UsersConference.Email,
                    Status = e.Status
                })
                .ToList();

            ViewData["InvitedResearchers"] = invitedResearchers;

            return View(researchers);
        }

        //InviteLogic
        [HttpPost]
        public async Task<IActionResult> ProcessInvitation(List<string> selectedResearchers)
        {
            if (selectedResearchers == null || !selectedResearchers.Any())
            {
                TempData["ErrorMessage"] = "No researchers were selected.";
                return RedirectToAction("ViewInvitedEvaluators");
            }

            foreach (var userId in selectedResearchers)
            {
                var userRole = _context.UserConferenceRoles
                    .Include(ucr => ucr.UsersConference)
                    .FirstOrDefault(ucr => ucr.UserId == userId);

                if (userRole != null)
                {
                    var evaluator = new EvaluatorInfo
                    {
                        EvaluatorId = Guid.NewGuid().ToString(),
                        UserRoleId = userRole.UserRoleId,
                        Status = "Pending"
                    };

                    _context.EvaluatorInfo.Add(evaluator);

                    var subject = "Invitation to Become an Evaluator";
                    var body = $@"
                    Dear {userRole.UsersConference.FirstName} {userRole.UsersConference.LastName},

                    You are invited to become an evaluator at the RCBACONFERENCE. Kindly login to your account to accept the invitation.

                    Best Regards,
                    RCBACONFERENCE Team";

                    await SendEmailAsync(userRole.UsersConference.Email, subject, body);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Invitations sent successfully.";
            return RedirectToAction("ViewInvitedEvaluators");
        }

        //Send Email to Invited ppl
        private async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("RCBACONFERENCE Notification", "rmocrdlnotification@gmail.com"));
                message.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                // Use the fully qualified name for MailKit's SmtpClient
                using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("rmocrdlnotification@gmail.com", "uhflpdedbetywhxu");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation("Email sent successfully to {RecipientEmail}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to {RecipientEmail}", recipientEmail);
            }

        }

        [HttpGet]
        public IActionResult GenerateReport()
        {
            var researchEvents = _context.ResearchEvent
                .Select(e => new { e.ResearchEventId, e.EventName })
                .ToList();
            ViewBag.ResearchEvents = researchEvents;
            return View();
        }

        [HttpPost]
        public IActionResult GenerateReport(string fileName, string reportType, string researchEventId)
        {
            if (reportType == "Receipts")
            {
                var stream = _reportService.GenerateReceiptReport(researchEventId);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
            }
            else if (reportType == "ResearchPapers")
            {
                var stream = _reportService.GenerateResearchPapersReport(researchEventId);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
            }

            return BadRequest("Invalid report type.");
        }

    }
}