using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace WelcomeEmail.Controllers;

    [Route("api/controller")]
    [ApiController]
public class EmailController : ControllerBase
{
    [HttpPost]
    public IActionResult SendEmail(string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("antal.zsofia84@gmail.com"));
        email.To.Add(MailboxAddress.Parse("antal.zsofia84@gmail.com"));
        email.Subject = "Test email Subject";
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("antal.zsofia84@gmail.com", "0Kakipisifing00");
        smtp.Send(email);
        smtp.Disconnect(true);

        return Ok();
    }
}