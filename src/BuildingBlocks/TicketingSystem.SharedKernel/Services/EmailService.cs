using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using TicketingSystem.SharedKernel.DTOs;

namespace TicketingSystem.SharedKernel.Services
{
    /// <summary>
    /// Email service implementation
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;
        private readonly IResend _resend;

        public EmailService(ILogger<EmailService> logger, IConfiguration config, IResend resend)
        {
            _logger = logger;
            _config = config;
            _resend = resend;
        }

        public async Task<SendEmailResponse> SendTicketEmailAsync(
            SendTicketEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new SendEmailResponse();

            try
            {
                _logger.LogInformation(
                    "Sending ticket email to {Email}. Order={OrderNumber}, Event={EventName}, TicketCount={TicketCount}",
                    request.RecipientEmail, request.OrderNumber, request.EventName, request.TicketCount);

                var subject = $"Your Tickets for {request.EventName}";
                var htmlBody = GenerateTicketEmailHtml(
                    request.RecipientName,
                    request.OrderNumber,
                    request.EventName,
                    request.EventDate,
                    request.VenueName,
                    request.TicketCount);

                var attachments = new List<SharedKernel.Services.EmailAttachment>
                {
                    new SharedKernel.Services.EmailAttachment(
                        FileName: $"Tickets-{request.OrderNumber}.pdf",
                        Content: request.PdfAttachment,
                        ContentType: "application/pdf")
                };

                var sendEmailRequest = new SendEmailRequest(
                    RecipientEmail: request.RecipientEmail,
                    Subject: subject,
                    HtmlBody: htmlBody,
                    Attachments: attachments);

                var result = await SendEmailAsync(sendEmailRequest, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Ticket email sent successfully to {Email}. MessageId={MessageId}",
                        request.RecipientEmail, result.MessageId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send ticket email to {Email}. Response={Response}",
                        request.RecipientEmail, result.Response);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending ticket email to {Email}",
                    request.RecipientEmail);

                response.IsSuccess = false;
                response.MessageId = string.Empty;
                response.Response = $"Error: {ex.Message}";
                return response;
            }
        }

        public async Task<SendEmailResponse> SendEmailAsync(
            SendEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new SendEmailResponse();

            try
            {
                var apiKey = _config["Resend:ApiKey"];

                var email = new EmailMessage
                {
                    From = _config["Resend:From"],
                    To = [request.RecipientEmail],
                    Subject = request.Subject,
                    HtmlBody = request.HtmlBody
                };

                if (request.Attachments != null && request.Attachments.Any())
                {
                    email.Attachments = request.Attachments.Select(a => new Resend.EmailAttachment
                    {
                        Filename = a.FileName,
                        Content = Convert.ToBase64String(a.Content),
                        ContentType = a.ContentType
                    }).ToList();
                }

                var result = await _resend.EmailSendAsync(email, cancellationToken);

                response.IsSuccess = true;
                response.MessageId = result.Content.ToString() ?? string.Empty;
                response.Response = "Email sent successfully";

                _logger.LogInformation(
                    "Email sent via Resend to {Email}. MessageId={MessageId}",
                    request.RecipientEmail, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", request.RecipientEmail);
                response.IsSuccess = false;
                response.MessageId = string.Empty;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }

        private static string GenerateTicketEmailHtml(
            string recipientName,
            string orderNumber,
            string eventName,
            DateTime eventDate,
            string venueName,
            int ticketCount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        .event-details {{
            background: white;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .button {{
            display: inline-block;
            background: #4CAF50;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 Your Tickets Are Ready!</h1>
    </div>
    
    <div class='content'>
        <p>Hi {recipientName},</p>
        
        <p>Thank you for your order! Your tickets for <strong>{eventName}</strong> are attached to this email.</p>
        
        <div class='event-details'>
            <h2>Event Details</h2>
            <p><strong>Event:</strong> {eventName}</p>
            <p><strong>Date:</strong> {eventDate:dddd, MMMM dd, yyyy}</p>
            <p><strong>Time:</strong> {eventDate:hh:mm tt}</p>
            <p><strong>Venue:</strong> {venueName}</p>
            <p><strong>Tickets:</strong> {ticketCount}</p>
            <p><strong>Order Number:</strong> {orderNumber}</p>
        </div>
        
        <p><strong>Important:</strong></p>
        <ul>
            <li>Please download and save your tickets</li>
            <li>Present the QR code at the entrance</li>
            <li>Arrive early to avoid queues</li>
            <li>Keep your tickets safe and do not share</li>
        </ul>
        
        <p>If you have any questions, please contact our support team.</p>
        
        <p>See you at the event! 🎊</p>
    </div>
    
    <div class='footer'>
        <p>This email was sent by Your Ticketing Platform</p>
        <p>Order #{orderNumber}</p>
    </div>
</body>
</html>";
        }

        public async Task<SendEmailResponse> SendWelcomeEmailAsync(string recipientEmail, string recipientName, CancellationToken ct = default)
        {
            var html = $@"<h1>Welcome, {recipientName}!</h1>
                  <p>Your account has been created successfully.</p>";

            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);

            return await SendEmailAsync(emailRequest, ct);
        }

        public async Task<SendEmailResponse> SendEmailVerificationAsync(string recipientEmail, string recipientName, string verificationLink, CancellationToken ct = default)
        {

            var html =  GenerateEmailVerificationHtml(recipientName, recipientEmail, verificationLink);

            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);

            return await SendEmailAsync(emailRequest, ct);
        }

        private static string GenerateEmailVerificationHtml(string recipientName,string recipientEmail,string verificationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        .details {{
            background: white;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .button {{
            display: inline-block;
            background: #4CAF50;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>✉️ Verify Your Email</h1>
    </div>

    <div class='content'>
        <p>Hi {recipientName},</p>

        <p>Thank you for registering! Please verify your email address to activate your account.</p>

        <div class='details'>
            <h2>Account Details</h2>
            <p><strong>Name:</strong> {recipientName}</p>
            <p><strong>Email:</strong> {recipientEmail}</p>
        </div>

        <p>Click the button below to verify your email address:</p>

        <a href='{verificationLink}' class='button'>Verify Email Address</a>

        <p><strong>Important:</strong></p>
        <ul>
            <li>This link expires in <strong>24 hours</strong></li>
            <li>If you did not create an account, ignore this email</li>
            <li>Never share this link with anyone</li>
        </ul>

        <p>If the button doesn't work, copy and paste this link into your browser:</p>
        <p style='word-break: break-all; color: #4CAF50;'>{verificationLink}</p>
    </div>

    <div class='footer'>
        <p>This email was sent by Your Ticketing Platform</p>
        <p>If you didn't register, please ignore this email.</p>
    </div>
</body>
</html>";
        }


        public async Task<SendEmailResponse> SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken ct = default)
        {
            var html = GeneratePasswordResetHtml(recipientName, recipientEmail, resetLink);


            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);

           
            return await SendEmailAsync(emailRequest, ct);

        }

        private static string GeneratePasswordResetHtml(string recipientName,string recipientEmail,string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: #e53935;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        .details {{
            background: white;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .button {{
            display: inline-block;
            background: #e53935;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🔐 Password Reset Request</h1>
    </div>

    <div class='content'>
        <p>Hi {recipientName},</p>

        <p>We received a request to reset the password for your account.</p>

        <div class='details'>
            <h2>Account Details</h2>
            <p><strong>Name:</strong> {recipientName}</p>
            <p><strong>Email:</strong> {recipientEmail}</p>
        </div>

        <p>Click the button below to reset your password:</p>

        <a href='{resetLink}' class='button'>Reset Password</a>

        <p><strong>Important:</strong></p>
        <ul>
            <li>This link expires in <strong>1 hour</strong></li>
            <li>If you did not request a password reset, ignore this email</li>
            <li>Your password will not change until you click the link above</li>
            <li>Never share this link with anyone</li>
        </ul>

        <p>If the button doesn't work, copy and paste this link into your browser:</p>
        <p style='word-break: break-all; color: #e53935;'>{resetLink}</p>
    </div>

    <div class='footer'>
        <p>This email was sent by Your Ticketing Platform</p>
        <p>If you didn't request this, please secure your account immediately.</p>
    </div>
</body>
</html>";
        }


        public async Task<SendEmailResponse> SendAccountLockedAsync(string recipientEmail, string recipientName, DateTime unlocksAt, CancellationToken ct = default)
        {
            var html = GenerateAccountLockedHtml(recipientName, recipientEmail, unlocksAt);


            var emailRequest = new SendEmailRequest(
               RecipientEmail: recipientEmail,
               Subject: "Welcome to Our Service!",
               HtmlBody: html,
               Attachments: null);

            return await SendEmailAsync(emailRequest, ct);
        }

        private static string GenerateAccountLockedHtml(string recipientName,string recipientEmail,DateTime unlocksAt)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: #f57c00;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        .details {{
            background: white;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>⚠️ Account Locked</h1>
    </div>

    <div class='content'>
        <p>Hi {recipientName},</p>

        <p>Your account has been temporarily locked due to too many failed login attempts.</p>

        <div class='details'>
            <h2>Account Details</h2>
            <p><strong>Name:</strong> {recipientName}</p>
            <p><strong>Email:</strong> {recipientEmail}</p>
            <p><strong>Locked Until:</strong> {unlocksAt:dddd, MMMM dd, yyyy} at {unlocksAt:hh:mm tt} UTC</p>
        </div>

        <p><strong>What to do:</strong></p>
        <ul>
            <li>Wait until <strong>{unlocksAt:hh:mm tt UTC}</strong> and try again</li>
            <li>If you forgot your password, use the forgot password option</li>
            <li>If this wasn't you, secure your account immediately</li>
        </ul>

        <p>If you believe this is a mistake or suspect unauthorized access, please contact our support team immediately.</p>
    </div>

    <div class='footer'>
        <p>This email was sent by Your Ticketing Platform</p>
        <p>If this wasn't you, please contact support immediately.</p>
    </div>
</body>
</html>";
        }

        public async Task<SendEmailResponse> SendOrderConfirmationEmailAsync(string recipientEmail, string recipientName, string orderNumber, string eventName, DateTime eventDate, string venueName, decimal totalAmount, int ticketCount, CancellationToken ct = default)
        {
            var html = GeneratePasswordResetHtml(recipientName, recipientEmail, resetLink);


            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);


            return await SendEmailAsync(emailRequest, ct);
        }

        public async Task<SendEmailResponse> SendOrderCancelledEmailAsync(string recipientEmail, string recipientName, string orderNumber, string eventName, string cancellationReason, CancellationToken ct = default)
        {
            var html = GeneratePasswordResetHtml(recipientName, recipientEmail, resetLink);


            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);


            return await SendEmailAsync(emailRequest, ct);
        }

        public async Task<SendEmailResponse> SendOrderExpiredEmailAsync(string recipientEmail, string recipientName, string orderNumber, string eventName, CancellationToken ct = default)
        {
            var html = GeneratePasswordResetHtml(recipientName, recipientEmail, resetLink);


            var emailRequest = new SendEmailRequest(
                RecipientEmail: recipientEmail,
                Subject: "Welcome to Our Service!",
                HtmlBody: html,
                Attachments: null);


            return await SendEmailAsync(emailRequest, ct);
        }
    }
}
