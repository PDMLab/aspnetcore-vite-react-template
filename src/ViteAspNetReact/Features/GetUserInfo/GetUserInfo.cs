using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Features.PrincipalExtensions;

namespace ViteAspNetReact.Features.GetUserInfo;

[Authorize]
public class GetUserInfoController : Controller
{
  [HttpGet, Route("/api/userinfo")]
  public Task<IActionResult> GetUserInfo(
    [FromServices] UserManager<AppUser> userManager
  )
  {
    return Task.FromResult<IActionResult>(
      Json(
        new
        {
          FirstName = User.GetFirstname(),
          LastName = User.GetLastname(),
          Subscription = User
            .GetSubscriptionId()
            .Value,
          Sub = User.GetSub()
        }
      )
    );
  }
}
