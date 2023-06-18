using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ViteAspNetReact.Controllers;

public class CustomerController : Controller
{
  [HttpPost, Route("/api/customer")]
  public IActionResult Index([FromBody] Customer customer)
  {
    if (!ModelState.IsValid)
      return Problem("Company name is required", null, 400);

    return Json(customer);
  }
}

public class Customer
{
  [Required(ErrorMessage = "Company name is required")]
  public string CompanyName { get; set; }
}
