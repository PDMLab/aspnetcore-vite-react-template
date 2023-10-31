using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;

namespace ViteAspNetReact.Features.DisableUser;

public class DisableUserController : Controller
{
}

public record UserDisabled
{
  public Guid Id { get; }
  public UserId UserId { get; }
  public string Email { get; }
  public string FirstName { get; }
  public string LastName { get; }
  public string TenantId { get; }
  public string Sub { get; }

  private UserDisabled(
    UserId userId,
    string email,
    string firstName,
    string lastName,
    string tenantId,
    string sub
  )
  {
    UserId = userId;
    Email = email;
    FirstName = firstName;
    LastName = lastName;
    TenantId = tenantId;
    Sub = sub;
  }

  public static UserDisabled Create(
    UserId userId,
    string email,
    string firstName,
    string lastName,
    string tenantId,
    string sub
  )
  {
    return new UserDisabled(
      userId,
      email,
      firstName,
      lastName,
      tenantId,
      sub
    );
  }

  public void Deconstruct(
    out Guid id,
    out UserId userId,
    out string email,
    out string firstName,
    out string lastName,
    out string tenantId,
    out string sub
  )
  {
    id = Id;
    userId = UserId;
    email = Email;
    firstName = FirstName;
    lastName = LastName;
    tenantId = TenantId;
    sub = Sub;
  }
}

public class DisabledUser : User
{
  private DisabledUser(
    Guid id,
    string email,
    string firstName,
    string lastName,
    string initials,
    Guid subscriptionId
  )
  {
    Id = id;
    Email = email;
    FirstName = firstName;
    LastName = lastName;
    Initials = initials;
    SubscriptionId = subscriptionId;
  }

  public static DisabledUser Create(
    UserDisabled disabled
  )
  {
    var initials = $"{disabled.FirstName[..1]}{disabled.LastName[..1]}";

    return new DisabledUser(
      disabled.Id,
      disabled.Email,
      disabled.FirstName,
      disabled.LastName,
      initials,
      Guid.Parse(
        disabled.TenantId
      )
    );
  }

  public override Guid Id { get; set; }
}

public static class UserProjectionConfiguration
{
  public static void UseDisabledUserProjections(
    this StoreOptions options
  )
  {
    options.Schema
      .For<User>()
      .AddSubClass<DisabledUser>();


    options.Projections.Snapshot<DisabledUser>(
      SnapshotLifecycle.Inline
    );
  }
}

public static class UsersConfig
{
  public static IServiceCollection AddDisableUser(
    this IServiceCollection services
  )
  {
    return services;
  }
}
