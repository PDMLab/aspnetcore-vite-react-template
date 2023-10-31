using Microsoft.Extensions.DependencyInjection;
using ViteAspNetReact.Core;

namespace ViteAspNetReact.IntegrationTests.Helpers;

public static class TestEmailTemplateServiceRegistration
{
  public static IServiceCollection UseTestEmailTemplateService(
    this IServiceCollection services
  )
  {
    var sender = new TestEmailTemplateService();
    return services.AddSingleton<IEmailTemplateService>(sender);
  }
}

public class TestEmailTemplateService : IEmailTemplateService
{
  public Task<string> RenderTemplate(
    EmailTemplate template
  )
  {
    var result = template switch
    {
      EmailTemplate.ConfirmEmail confirmEmail => confirmEmail.Code,
      _ => string.Empty
    };

    return Task.FromResult(result);
  }
}
