using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViteAspNetReact.Controllers;

[Authorize]
public class HelloController : Controller
{
  [HttpGet, Route("/api/hello")]
  public async Task<IActionResult> Index()
  {
    return Json(new { hello = User.Identity?.Name });
  }
}
