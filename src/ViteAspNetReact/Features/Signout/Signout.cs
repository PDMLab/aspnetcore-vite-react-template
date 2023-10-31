using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Areas.Identity.Data;

namespace ViteAspNetReact.Features.SignOut;

[Authorize]
public class SignOutController : Controller
{
  [HttpPost("/api/signout")]
  public async Task<IActionResult> Signout(
    [FromServices] SignInManager<AppUser> signInManager
  )
  {
    await signInManager.SignOutAsync();
    return Ok();
  }
}
