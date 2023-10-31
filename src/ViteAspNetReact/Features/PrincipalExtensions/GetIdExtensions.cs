using System.Security.Claims;

namespace ViteAspNetReact.Features.PrincipalExtensions;

public static class GetIdClaimsExtensions
{
  public static string? GetSub(
    this ClaimsPrincipal user
  ) => user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
    ?.Value;
}
