using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;
using RCBACONFERENCE.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace RCBACONFERENCE.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            EmailService emailService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult WelcomePage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password.Length < 6)
                {
                    ModelState.AddModelError("Password", "The password must be at least 6 characters long.");
                }

                if (HasInvalidCharacters(model.FirstName))
                {
                    ModelState.AddModelError("FirstName", "First name contains invalid characters. Only letters, spaces, and hyphens are allowed.");
                }
                if (model.MiddleName != null && HasInvalidCharacters(model.MiddleName))
                {
                    ModelState.AddModelError("MiddleName", "Middle name contains invalid characters. Only letters, spaces, and hyphens are allowed.");
                }
                if (HasInvalidCharacters(model.LastName))
                {
                    ModelState.AddModelError("LastName", "Last name contains invalid characters. Only letters, spaces, and hyphens are allowed.");
                }
                if (HasInvalidCharacters(model.Affiliation))
                {
                    ModelState.AddModelError("Affiliation", "Affiliation contains invalid characters. Only letters, spaces, and hyphens are allowed.");
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existingUser = await _context.UsersConference
                        .FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists. Please use a different email.");
                        return View(model);
                    }

                    var passwordHasher = new PasswordHasher<UsersConference>();
                    var user = new UsersConference
                    {
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Affiliation = model.Affiliation,
                        Classification = model.Classification,
                        CountryRegion = model.CountryRegion,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    string verificationCode = user.GenerateVerificationCode();
                    user.VerificationCode = verificationCode;
                    user.Password = passwordHasher.HashPassword(user, model.Password);

                    _context.UsersConference.Add(user);
                    await _context.SaveChangesAsync();

                    var researcherRole = await _context.ConferenceRoles
                        .FirstOrDefaultAsync(r => r.RoleName == "Researcher");

                    if (researcherRole == null)
                    {
                        throw new Exception("Researcher role not found in the system.");
                    }

                    var userRole = new UserConferenceRoles
                    {
                        UserId = user.UserId,
                        RoleId = researcherRole.RoleId,
                        UserRoleId = UserConferenceRoles.GenerateUserRoleId(researcherRole.RoleName)
                    };

                    _context.UserConferenceRoles.Add(userRole);
                    await _context.SaveChangesAsync();

                    try
                    {
                        await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send verification email: {ex.Message}");
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("Verify", new { email = user.Email });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                    return View(model);
                }
            }
            return View(model);
        }

        private bool HasInvalidCharacters(string input)
        {
            var regex = new Regex(@"^[a-zA-Z\s\-]+$");
            return !regex.IsMatch(input);
        }

        [HttpGet]
        public IActionResult Verify(string email)
        {
            return View(new VerifyViewModel { Email = email });
        }

        [HttpPost]
        public IActionResult Verify(VerifyViewModel model)
        {
            var user = _context.UsersConference.FirstOrDefault(u => u.Email == model.Email);
            if (user != null && user.VerificationCode == model.VerificationCode)
            {
                user.IsVerified = true;
                _context.SaveChanges();

                return RedirectToAction("Verified");
            }
            else
            {
                TempData["ErrorMessage"] = "Wrong verification code!";
                ModelState.AddModelError(string.Empty, "Invalid verification code.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerificationCodeForRegister(string email)
        {
            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            user.VerificationCode = user.GenerateVerificationCode();
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);

            TempData["Message"] = "A new verification code has been sent to your email.";
            return RedirectToAction("Verify", new { email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerificationCodeForResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is missing. Please retry the process.";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("VerifyResetPassword", new { email });
            }

            try
            {
                user.VerificationCode = user.GenerateVerificationCode();
                user.UpdatedAt = DateTime.Now;

                _context.Update(user);
                await _context.SaveChangesAsync();

                await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);

                TempData["Message"] = "A new verification code has been sent to reset your password.";
                return RedirectToAction("VerifyResetPassword", new { email });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resending verification code: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while resending the verification code. Please try again.";
                return RedirectToAction("VerifyResetPassword", new { email });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string email, string newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
            {
                TempData["ErrorMessage"] = "New email cannot be empty.";
                return RedirectToAction("Verify", new { email });
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Verify", new { email });
            }

            var existingUser = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == newEmail);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "The new email is already in use. Please use a different email.";
                return RedirectToAction("Verify", new { email });
            }

            user.Email = newEmail;
            user.VerificationCode = user.GenerateVerificationCode();
            user.UpdatedAt = DateTime.Now;

            _context.UsersConference.Update(user);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);
                TempData["Message"] = "Your email has been updated, and a new verification code has been sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send verification email to {user.Email}: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to send a verification email. Please try again.";
            }

            return RedirectToAction("Verify", new { email = user.Email });
        }

        public IActionResult Verified()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.UsersConference
                        .FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user != null)
                    {
                        var passwordHasher = new PasswordHasher<UsersConference>();
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            if (!user.IsVerified)
                            {
                                user.VerificationCode = user.GenerateVerificationCode();
                                user.UpdatedAt = DateTime.Now;

                                _context.Update(user);
                                await _context.SaveChangesAsync();

                                try
                                {
                                    await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);
                                    TempData["Message"] = "A verification code has been sent to your email.";
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to send verification email: {ex.Message}");
                                    TempData["ErrorMessage"] = "Failed to send verification email. Please try again.";
                                }

                                return RedirectToAction("Verify", new { email = user.Email });
                            }

                            // Get user roles
                            var userRoles = await _context.UserConferenceRoles
                                .Include(ur => ur.ConferenceRoles)
                                .Where(ur => ur.UserId == user.UserId)
                                .ToListAsync();

                            // Create claims for the user
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.GivenName, user.FirstName),
                                new Claim(ClaimTypes.Surname, user.LastName)
                            };

                            // Add role claims
                            foreach (var userRole in userRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, userRole.ConferenceRoles.RoleName));
                            }

                            // Create the identity
                            var claimsIdentity = new ClaimsIdentity(
                                claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            // Create the principal
                            var principal = new ClaimsPrincipal(claimsIdentity);

                            // Sign in the user
                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                principal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = true,
                                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                                });

                            _logger.LogInformation($"User {user.Email} logged in successfully");

                            // Redirect based on roles
                            var roleNames = userRoles.Select(ur => ur.ConferenceRoles.RoleName).ToList();

                            if (roleNames.Contains("Researcher") && roleNames.Contains("Evaluator"))
                            {
                                return RedirectToAction("SelectUser", "Account");
                            }
                            else if (roleNames.Contains("Researcher"))
                            {
                                return RedirectToAction("HomeResearcher", "Researcher");
                            }
                            else if (roleNames.Contains("Evaluator"))
                            {
                                return RedirectToAction("HomeEvaluator", "Evaluator");
                            }
                            else if (roleNames.Contains("Chief"))
                            {
                                return RedirectToAction("HomeChief", "Chief");
                            }

                            TempData["ErrorMessage"] = "User role is not recognized.";
                            return RedirectToAction("HomePage", "Home");
                        }
                    }

                    TempData["ErrorMessage"] = "Invalid email or password.";
                    _logger.LogWarning($"Failed login attempt for email: {model.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Login error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Please provide a valid email address.";
                return View();
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Email not found.";
                return View();
            }

            user.VerificationCode = user.GenerateVerificationCode();
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);
                TempData["Message"] = "A verification code has been sent to your email.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to send verification email. Please try again.";
                _logger.LogError($"Error sending email: {ex.Message}");
            }

            return RedirectToAction("VerifyResetPassword", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult VerifyResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new VerifyViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public IActionResult VerifyResetPassword(VerifyViewModel model)
        {
            _logger.LogInformation($"VerifyResetPassword POST called with Email: {model.Email}, Verification Code: {model.VerificationCode}");

            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Email is missing. Please retry the process.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.VerificationCode))
            {
                TempData["ErrorMessage"] = "Verification code is required.";
                return View(model);
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                _logger.LogWarning($"User not found for Email: {model.Email}");
                return View(model);
            }

            string enteredCode = model.VerificationCode.Trim().ToUpper();
            string storedCode = user.VerificationCode.Trim().ToUpper();

            _logger.LogInformation($"Comparing entered code: '{enteredCode}' with stored code: '{storedCode}'");

            if (!string.Equals(enteredCode, storedCode, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Invalid verification code.";
                _logger.LogWarning($"Verification code mismatch for Email: {model.Email}. Expected: {storedCode}, Entered: {enteredCode}");
                return View(model);
            }

            TempData["Message"] = "Verification successful. You can now reset your password.";
            _logger.LogInformation($"Verification successful for Email: {model.Email}");
            return RedirectToAction("ResetPassword", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is missing. Please retry the process.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Email = email };
            _logger.LogInformation($"Rendering ResetPassword view with Email: {email}");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {

            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Email is missing. Please retry the process.";
                _logger.LogError("Email is missing in ResetPassword POST action.");
                return RedirectToAction("ForgotPassword");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Validation Error: {error.ErrorMessage}");
                }

                TempData["ErrorMessage"] = "There were validation errors. Please check your input.";
                return View(model);
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                _logger.LogWarning($"User not found for Email: {model.Email}");
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }

            try
            {
                var passwordHasher = new PasswordHasher<UsersConference>();

                var verificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, model.NewPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    TempData["ErrorMessage"] = "The new password cannot be the same as your current password. Please choose a different password.";
                    _logger.LogWarning($"User {user.Email} attempted to reset their password to the same as the current password.");
                    return View(model);
                }

                user.Password = passwordHasher.HashPassword(user, model.NewPassword);
                user.UpdatedAt = DateTime.Now;

                _context.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password successfully updated for User ID: {user.UserId}");
                TempData["Message"] = "Your password has been reset successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating password: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while resetting your password. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("WelcomePage", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ProfileSettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty.");
                return Unauthorized();
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileSettingsViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Affiliation = user.Affiliation,
                CountryRegion = user.CountryRegion
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProfileSettings(ProfileSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.MiddleName = model.MiddleName;
                user.LastName = model.LastName;
                user.Affiliation = model.Affiliation;
                user.CountryRegion = model.CountryRegion;

                _context.UsersConference.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("ProfileSettings");
            }

            TempData["ErrorMessage"] = "Failed to update your profile. Please check the inputs.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.UsersConference.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            var passwordHasher = new PasswordHasher<UsersConference>();
            var passwordVerification = passwordHasher.VerifyHashedPassword(user, user.Password, CurrentPassword);

            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction("ProfileSettings");
            }

            var newPasswordVerification = passwordHasher.VerifyHashedPassword(user, user.Password, NewPassword);
            if (newPasswordVerification == PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "New password cannot be the same as the current password.";
                return RedirectToAction("ProfileSettings");
            }

            if (NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
                return RedirectToAction("ProfileSettings");
            }

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation password do not match.";
                return RedirectToAction("ProfileSettings");
            }

            user.Password = passwordHasher.HashPassword(user, NewPassword);
            _context.UsersConference.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully!";
            return RedirectToAction("ProfileSettings");
        }

        [HttpPost]
        public IActionResult VerifyPassword(string Password)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized request." });
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var passwordHasher = new PasswordHasher<UsersConference>();

            if (string.IsNullOrWhiteSpace(Password))
            {
                return Json(new { success = false, message = "Enter your password." });
            }

            var passwordVerification = passwordHasher.VerifyHashedPassword(user, user.Password, Password);

            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                return Json(new { success = false, message = "Incorrect password." });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized request." });
            }

            var user = _context.UsersConference.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            _context.UsersConference.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your account has been successfully deleted.";

            return Json(new { success = true });
        }

        public IActionResult SelectUser()
        {
            return View();
        }

    }
}
