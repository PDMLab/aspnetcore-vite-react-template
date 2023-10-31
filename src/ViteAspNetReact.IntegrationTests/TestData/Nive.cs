using Alba;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features;
using ViteAspNetReact.Features.InviteUser;
using ViteAspNetReact.Features.RegisterUser;

namespace ViteAspNetReact.IntegrationTests.TestData;

public static class Nive
{
  public static string CompanyName => "Nive, LLC";

  public static class EmmaJohnson
  {
    public static string FirstName => "Emma";
    public static string LastName => "Johnson";
    public static string EMail => "dev-mail+emma.johnson.nive@nive.local";
    public static string UserName => EMail;
  }

  public static class MarkTurner
  {
    public static string FirstName => "Mark";
    public static string LastName => "Turn";
    public static string EMail => "dev-mail+mark.turner.nive@nive.local";
    public static string UserName => EMail;
  }

  public static class LiamSmith
  {
    public static string FirstName => "Liam";
    public static string LastName => "Smith";
    public static string EMail => "dev-mail+liam.smith.nive@nive.local";
    public static string UserName => EMail;
  }


  public static async Task<SubscriptionId> RegisterNive(
    this IAlbaHost host
  )

  {
    var registration = new Registration()
    {
      FirstName = EmmaJohnson.FirstName,
      LastName = EmmaJohnson.LastName,
      CompanyName = CompanyName,
      Email = EmmaJohnson.EMail,
      Password = "Pa$$w0rd",
      TermsAccepted = true
    };

    var userStore = host.Services.GetService<IUserStore<AppUser>>() ?? throw new NullReferenceException();
    var userManager = host.Services.GetService<UserManager<AppUser>>() ?? throw new NullReferenceException();
    var globalStore = host.Services.GetService<IGlobalStore>() ?? throw new NullReferenceException();
    var subscriptionsStore = host.Services.GetService<ISubscribersStore>() ?? throw new NullReferenceException();
    var eventStoreConfiguration =
      host.Services.GetService<EventStoreConfiguration>() ?? throw new NullReferenceException();
    var idGenerator = host.Services.GetService<IIdGenerator>() ?? throw new NullReferenceException();

    var handler = new RegisterUserHandler(
      userStore,
      userManager,
      globalStore,
      subscriptionsStore,
      eventStoreConfiguration,
      idGenerator,
      GetEmailStore(userManager, userStore)
    );
    return await handler.Handle(registration, CancellationToken.None);
  }

  public static async Task InviteTeam(
    this IAlbaHost host,
    CancellationToken token
  )
  {
    var inviteUser = new InviteUser(
      MarkTurner.FirstName,
      MarkTurner.LastName,
      new EmailInputModel()
      {
        Value = MarkTurner.EMail
      },
      null
    );
    var userManager = host.Services.GetService<UserManager<AppUser>>() ?? throw new NullReferenceException();
    var userStore = host.Services.GetService<IUserStore<AppUser>>() ?? throw new NullReferenceException();
    var globalStore = host.Services.GetService<IGlobalStore>() ?? throw new NullReferenceException();
    var eventStoreConfiguration =
      host.Services.GetService<EventStoreConfiguration>() ?? throw new NullReferenceException();
    var emailSender = host.Services.GetService<IEmailSender>() ?? throw new NullReferenceException();

    var handler = new InviteUserHandler(
      userManager,
      userStore,
      globalStore,
      eventStoreConfiguration,
      emailSender
    );
    await using var session = globalStore.QuerySession();
    var emmaJohnson = session.Query<User>()
      .FirstOrDefault(u => u.Email == EmmaJohnson.EMail);
    await handler.Handle(
      inviteUser,
      emmaJohnson,
      token
    );
  }

  private static IUserEmailStore<AppUser> GetEmailStore(
    UserManager<AppUser> userManager,
    IUserStore<AppUser> userStore
  )
  {
    if (!userManager.SupportsUserEmail)
    {
      throw new NotSupportedException("The default UI requires a user store with email support.");
    }

    return (IUserEmailStore<AppUser>)userStore;
  }
}
