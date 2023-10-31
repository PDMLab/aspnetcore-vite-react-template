// TODO: Einladung per Id akzeptieren

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;

namespace ViteAspNetReact.Features.AcceptUserInvitation;

public class AcceptUserInvitationController : Controller
{
  [HttpPost("/api/invitations/{id:guid}/accept")]
  public Task<IActionResult> AcceptUserInvitation(
    [FromRoute] Guid id
    
  )
  {
    return Task.FromResult<IActionResult>(Ok());
  }
}

public class AcceptUserInvitationHandler
{
  private readonly UserManager<AppUser> _userManager;
  private readonly IGlobalStore _globalStore;
  private readonly ISubscribersStore _subscribersStore;

  public AcceptUserInvitationHandler(
    UserManager<AppUser> userManager,
    IGlobalStore globalStore,
    ISubscribersStore subscribersStore
    )
  {
    _userManager = userManager;
    _globalStore = globalStore;
    _subscribersStore = subscribersStore;
  }
  
  public async Task Handle(
    UserId userId,
    CancellationToken token
  )
  {
    
  } 
}

public record UserInvitationAccepted(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string Sub,
  string TenantId,
  DateTimeOffset On
);
