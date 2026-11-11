using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NewProject.Api.Services;

public class GmailEmailSender
{
    private readonly IConfiguration _configuration;

    public GmailEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendCodeAsync(
        string email,
        string code,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection("Email");

        var message = new MimeMessage(); //new email message

        message.From.Add(new MailboxAddress(
            section["SenderName"] ?? "NewProject",
            section["Username"]!));

        message.To.Add(MailboxAddress.Parse(email)); 

        message.Subject = purpose == "Registration"
            ? "Verify your email"
            : "Password reset verification";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Hello,

                Your verification code is: {code}

                This code expires in 10 minutes.
                If you did not request this, you can ignore this email.
                """
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            section["Host"] ?? "smtp.gmail.com",
            int.Parse(section["Port"] ?? "587"), //convert string to int
            SecureSocketOptions.StartTls,  // Use StartTLS for secure connection
            cancellationToken);

        await smtp.AuthenticateAsync(
            section["Username"],
            section["AppPassword"],
            cancellationToken);

        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}