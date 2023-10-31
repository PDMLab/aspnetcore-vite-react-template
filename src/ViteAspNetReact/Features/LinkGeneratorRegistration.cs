namespace ViteAspNetReact.Features;

public class LinkGeneratorRegistration
{
  private readonly LinkGenerator? _linkGenerator;

  public LinkGeneratorRegistration(
    IHttpContextAccessor contextAccessor,
    LinkGenerator linkGenerator
  )
  {
    _linkGenerator = contextAccessor.HttpContext?.RequestServices.GetRequiredService<LinkGenerator>();
  }

  public LinkGenerator? LinkGenerator => _linkGenerator;
}
