using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        using var smtpClient = new SmtpClient(emailSettings["Host"], int.Parse(emailSettings["Port"]))
        {
            EnableSsl = true,
            UseDefaultCredentials = false, 
            Credentials = new NetworkCredential(
                emailSettings["From"],
                emailSettings["AppPassword"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailSettings["From"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
