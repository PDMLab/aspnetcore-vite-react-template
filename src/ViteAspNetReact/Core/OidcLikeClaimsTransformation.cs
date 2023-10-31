using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using ViteAspNetReact.Areas.Identity.Data;

namespace ViteAspNetReact.Core;

public class OidcLikeClaimsTransformation : IClaimsTransformation
{
  private readonly UserManager<AppUser> _userManager;

  public OidcLikeClaimsTransformation(UserManager<AppUser> userManager)
  {
    _userManager = userManager;
  }

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    var claimsIdentity = new ClaimsIdentity();
    const string subClaimType = "sub";
    if (!principal.HasClaim(claim => claim.Type == subClaimType))
    {
      claimsIdentity.AddClaim(
        new Claim(
          subClaimType,
          _userManager.GetUserId(principal)
        )
      );
    }

    if (!principal.HasClaim(claim => claim.Type == ClaimTypes.GivenName))
    {
      var user = await _userManager.GetUserAsync(principal);
      if (user is null) return await Task.FromResult(principal);
      claimsIdentity.AddClaim(
        new Claim(
          ClaimTypes.GivenName,
          user.FirstName!
        )
      );
    }

    if (!principal.HasClaim(claim => claim.Type == ClaimTypes.Surname))
    {
      var user = await _userManager.GetUserAsync(principal);
      claimsIdentity.AddClaim(
        new Claim(
          ClaimTypes.Surname,
          user.LastName!
        )
      );
    }

    principal.AddIdentity(claimsIdentity);
    return principal;
  }
}
