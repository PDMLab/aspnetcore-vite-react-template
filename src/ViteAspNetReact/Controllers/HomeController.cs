using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Models;

namespace ViteAspNetReact.Controllers;


[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ILogger<HomeController> logger
    )
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    // [Route("/api/hello")]
    // public IActionResult Index()
    // {
    //     return Json(
    //         new
    //         {
    //             name = "Alex"
    //         }
    //     );
    // }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true
    )]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            }
        );
    }
}
