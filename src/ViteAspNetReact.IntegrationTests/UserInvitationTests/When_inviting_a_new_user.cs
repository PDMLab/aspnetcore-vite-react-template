using Alba;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features;
using ViteAspNetReact.Features.GetInvitedUsers;
using ViteAspNetReact.IntegrationTests.Helpers;
using ViteAspNetReact.IntegrationTests.TestData;

namespace ViteAspNetReact.IntegrationTests.UserInvitationTests;

public class When_inviting_a_new_user : IAsyncLifetime
{
  private readonly ITestOutputHelper _testOutputHelper;
  private TestServices? _testServices;
  private IAlbaHost? _host;
  private SubscriptionId? _subscriptionId;
  private IGlobalStore? _globalStore;
  private User? _markTurnerApplicationUser;
  private UserManager<AppUser>? _userManager;
  private AppUser? _markTurnerIdentityUser;
  private TestEmailSender? _emailSender;

  public When_inviting_a_new_user(
    ITestOutputHelper testOutputHelper
  ) => _testOutputHelper = testOutputHelper;

  public async Task InitializeAsync()
  {
    _testServices = new TestServices();
    var builder = await _testServices.GetTestHostBuilder();
    _host = await builder.StartAlbaAsync();
    await _host.MigrateIdentityDatabase();

    _subscriptionId = await _host.RegisterNive();
    await _host.InviteTeam(CancellationToken.None);

    _globalStore = _host.Services.GetService<IGlobalStore>();
    _userManager = _host.Services.GetService<UserManager<AppUser>>();
    await using var session = _globalStore?.QuerySession();
    _markTurnerApplicationUser = session?.Query<InvitedUser>()
      .FirstOrDefault(u => u.Email == Nive.MarkTurner.UserName);
    _markTurnerIdentityUser =
      _userManager?.Users.FirstOrDefault(u => u.UserName == Nive.MarkTurner.UserName);
    _emailSender = _host.Services.GetService<IEmailSender>() as TestEmailSender ?? throw new ArgumentNullException();
  }

  [Fact]
  public void should_invite_mark_turner()
  {
    _markTurnerIdentityUser.ShouldNotBeNull();
    _markTurnerApplicationUser.ShouldNotBeNull();
  }

  [Fact]
  public void should_send_invitation_email()
  {
    _emailSender.SentEmails.ShouldContain(e => e.Email == _markTurnerApplicationUser.Email);
  }


  public async Task DisposeAsync()
  {
    // await _testServices?.DropSubscriberTestDatabase(_subscriptionId);
    // await _testServices.DropGlobalTestDatabase();
  }
}
