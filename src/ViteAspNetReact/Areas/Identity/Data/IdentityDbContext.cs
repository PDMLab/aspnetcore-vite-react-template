using ViteAspNetReact.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ViteAspNetReact.Areas.Identity.Data;

public class IdentityDbContext : IdentityDbContext<AppUser>
{
  public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : base(options)
  {
  }
}
