using System.Security.Claims;
using ViteAspNetReact.Core;

namespace ViteAspNetReact.Features.PrincipalExtensions;

public static class GetUserInitialsExtensions
{
  public static string GetInitials(this ClaimsPrincipal user)
  {
    var firstName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)
      ?.Value;
    var lastName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)
      ?.Value;
    return $"{firstName?[..1]}{lastName?[..1]}";
  }
}

public static class ClaimPrincipalExtensions
{
  public static SubscriptionId GetSubscriptionId(
    this ClaimsPrincipal principal
  )
  {
    return new SubscriptionId(
      Guid.Parse(
        principal.GetTenantIdClaim()
          .Value
      )
    );
  }
}

public static class ClaimExtensions
{
  public static Claim GetTenantIdClaim(
    this ClaimsPrincipal principal
  )
  {
    // TODO: must be outside Users feature
    return principal.Claims.First(c => c.Type == Constants.TenantIdClaimName);
  }
}
