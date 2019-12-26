using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace TokenService.Services.EmailServices
{
  public interface ISendEmailService
  {
    Task SendEmail(string emailAddress, string subject, string htmlBody);
  }
  public sealed class SendEmailService:ISendEmailService
  {
    private readonly IConfiguration configuration;

    // config Data
    public string SourceAccount { get; set; } = "";
    public string SmtpServer { get; set; } = "";
    public string Password { get; set; } = "";
    public int SmtpPort { get; set; }

    private readonly ILogger log;
    
    public SendEmailService(ILogger<SendEmailService> log, IConfiguration configuration)
     {
      this.log = log;
      configuration.GetSection("Email").Bind(this);
     }

    public Task SendEmail(string emailAddress, string subject, string htmlBody)
    {
      log.Log(LogLevel.Information, $"Email sent to {emailAddress}: Subject");
      return Send(CreateMimeMessage(emailAddress, subject, htmlBody));
    }
    
    private async Task Send(MimeMessage mimeMessage)
    {
      using var client = new SmtpClient 
        {ServerCertificateValidationCallback = AcceptAnySSLCertificate};
      await client.ConnectAsync(SmtpServer, SmtpPort, true);
      await client.AuthenticateAsync(SourceAccount, Password);
      await client.SendAsync(mimeMessage);
      await client.DisconnectAsync(true);
    }

    private bool AcceptAnySSLCertificate(object s, X509Certificate c, X509Chain h, SslPolicyErrors e) 
      => true;

    private MimeMessage CreateMimeMessage(string email, string subject, string htmlMessage)
    {
      var mimeMessage = new MimeMessage();
      mimeMessage.From.Add(new MailboxAddress("CapWeb Account Robot", SourceAccount));
      mimeMessage.To.Add(new MailboxAddress(email));
      mimeMessage.Subject = subject;
      mimeMessage.Body = new TextPart("html"){Text = htmlMessage};
      return mimeMessage;
    }
  }
}