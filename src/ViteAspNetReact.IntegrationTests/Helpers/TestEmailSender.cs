using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ViteAspNetReact.IntegrationTests.Helpers;

public class SentEmail
{
  public SentEmail(
    string email,
    string subject,
    string message
  )
  {
    Email = email;
    Subject = subject;
    Message = message;
  }


  public string Subject { get; set; }
  public string Email { get; set; }
  public string Message { get; set; }
}

public static class TestEmailSenderRegistration
{
  public static IServiceCollection UseTestEmailSender(
    this IServiceCollection services
  )
  {
    var sender = new TestEmailSender();
    return services.AddSingleton<IEmailSender>(sender);
  }
}

public class TestEmailSender : IEmailSender
{
  public List<SentEmail> SentEmails { get; set; } = new();

  public Task SendEmailAsync(
    string email,
    string subject,
    string htmlMessage
  )
  {
    SentEmails.Add(
      new(
        email,
        subject,
        htmlMessage
      )
    );
    return Task.CompletedTask;
  }
}
