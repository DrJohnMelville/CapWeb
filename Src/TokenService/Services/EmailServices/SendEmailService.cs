using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
      return Task.CompletedTask;
    }
  }
}