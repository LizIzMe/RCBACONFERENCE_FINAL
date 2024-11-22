using System.Net;
using System.Net.Mail;

namespace RCBACONFERENCE.Services
{
    public class EmailService
    {
        private readonly string _emailAddress = "rmocrdlnotification@gmail.com";
        private readonly string _password = "uhfl pded bety whxu";
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendVerificationEmailAsync(string recipientEmail, string verificationCode)
        {
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_emailAddress, _password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailAddress, "RCBA Conference"),
                    Subject = "Email Verification - RCBA Conference",
                    Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Email Verification</h2>
                    <p>Thank you for registering for the RCBA Conference.</p>
                    <p>Your verification code is: <strong>{verificationCode}</strong></p>
                    <p>Please enter this code to verify your email address.</p>
                    <p>If you didn't request this verification, please ignore this email.</p>
                </body>
                </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Verification email sent to {RecipientEmail}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send email to {RecipientEmail}: {ErrorMessage}", recipientEmail, ex.Message);
                throw;
            }
        }

    }
}
