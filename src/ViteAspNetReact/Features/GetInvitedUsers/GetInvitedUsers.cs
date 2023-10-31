using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features.InviteUser;
using ViteAspNetReact.Features.PrincipalExtensions;
using ViteAspNetReact.Features.RegisterUser;

namespace ViteAspNetReact.Features.GetInvitedUsers;

public class GetInvitedUsersController : Controller
{
  public async Task<IActionResult> GetInvitedUsers(
    [FromServices] ISubscribersStore subscribersStore,
    [FromServices] EventStoreConfiguration storeConfiguration
  )
  {
    var subscriptionId = User.GetSubscriptionId();
    await using var session = subscribersStore.QuerySession(
      EventStore.GetDefaultEventStoreId(storeConfiguration)
    );
    var users = session.Query<InvitedUser>()
      .Where(u => u.SubscriptionId == subscriptionId.Value)
      .ToList()
      .AsReadOnly();

    // TODO: View
    return Ok();
  }
}

public static class UserProjectionConfiguration
{
  public static void UseInvitedUserProjections(
    this StoreOptions options
  )
  {
    options.Schema
      .For<User>()
      .AddSubClass<InvitedUser>();


    options.Projections.Snapshot<InvitedUser>(
      SnapshotLifecycle.Inline
    );
  }
}

public class InvitedUser : User
{
  private InvitedUser(
    string email,
    string firstName,
    string lastName,
    string initials,
    string sub,
    Guid subscriptionId
  )
  {
    Email = email;
    FirstName = firstName;
    LastName = lastName;
    Initials = initials;
    SubscriptionId = subscriptionId;
    Sub = sub;
  }

  public static InvitedUser Create(
    UserInvited invited
  )
  {
    var initials = $"{invited.FirstName[..1]}{invited.LastName[..1]}";

    return new InvitedUser(
      invited.Email,
      invited.FirstName,
      invited.LastName,
      initials,
      invited.Sub,
      Guid.Parse(
        invited.TenantId
      )
    );
  }

  // TODO: must be invitation accepted
  internal bool ShouldDelete(
    RegistrationConfirmed registrationConfirmed
  ) => true;

  public override Guid Id { get; set; }
}
