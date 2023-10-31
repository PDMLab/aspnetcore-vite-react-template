namespace ViteAspNetReact.Core;

public abstract record EmailTemplate
{
  public record ConfirmEmail(string Email, string Code) : EmailTemplate;
}


public interface IEmailTemplateService
{
  Task<string> RenderTemplate(EmailTemplate template);
}

public class RazorEmailTemplateService
{
  public RazorEmailTemplateService(LinkGenerator linkGenerator)
  {
  }
}
