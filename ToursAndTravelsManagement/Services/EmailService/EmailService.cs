using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

namespace ToursAndTravelsManagement.Services.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.SMTPUsername, _emailSettings.SMTPPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Your Name", "your-email@example.com"));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.TextBody = body;

        if (attachment != null)
        {
            builder.Attachments.Add("report.pdf", attachment, new ContentType("application", "pdf"));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailSettings.SMTPUsername, _emailSettings.SMTPPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendTicketEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Your Name", "your-email@example.com"));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.TextBody = body;

        if (attachment != null && attachment.Length > 0)
        {
            builder.Attachments.Add("ticket.pdf", attachment, new ContentType("application", "pdf"));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailSettings.SMTPUsername, _emailSettings.SMTPPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}