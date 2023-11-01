using System.Security.Claims;
using Alba;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features;
using ViteAspNetReact.Features.PrincipalExtensions;
using ViteAspNetReact.Features.RegisterEmployee;
using ViteAspNetReact.Features.RegisterUser;
using ViteAspNetReact.Features.SetOrganizationAddress;
using ViteAspNetReact.IntegrationTests.Helpers;
using ViteAspNetReact.IntegrationTests.TestData;

namespace ViteAspNetReact.IntegrationTests.SubscriptionTests;

public class When_setting_up_subscription_of_nive_crm : IAsyncLifetime
{
  private readonly ITestOutputHelper _testOutputHelper;
  private IAlbaHost _host;
  private AppUser? _identityuser;
  private TestServices _testServices;
  private SubscriptionId _subscriptionId;
  private Subscription? _subscription;
  private Organization? _organization;
  private Employee? _employee;
  private User? _subscriptionUser;

  public When_setting_up_subscription_of_nive_crm(
    ITestOutputHelper testOutputHelper
  ) =>
    _testOutputHelper = testOutputHelper;

  public async Task InitializeAsync()
  {
    _testServices = new TestServices();
    var hostBuilder = await _testServices.GetTestHostBuilder(_testOutputHelper);
    _host = await hostBuilder.StartAlbaAsync();

    await _host.MigrateIdentityDatabase();

    await Nive.RegisterNive(_host);

    var userManager = _host.Services.GetService<UserManager<AppUser>>() ?? throw new NullReferenceException();
    _identityuser = await userManager.FindByEmailAsync(Nive.EmmaJohnson.UserName);
    var claimsAsync = await userManager.GetClaimsAsync(_identityuser);
    var p = new ClaimsPrincipal(new ClaimsIdentity(claimsAsync));
    _subscriptionId = p.GetSubscriptionId();

    var globalStore = _host.Services.GetService<IGlobalStore>() ?? throw new NullReferenceException();
    await using var globalQuerySession = globalStore.QuerySession();

    _subscription = globalQuerySession.Load<Subscription>(_subscriptionId.Value);

    var subscriptionStore = _host.Services.GetService<ISubscribersStore>() ?? throw new NullReferenceException();
    await using var subscriptionQuerySession =
      subscriptionStore.QuerySession(EventStore.GetSubscriberEventStoreId(_subscriptionId));

    _organization = subscriptionQuerySession.Load<Organization>(
      OrganizationId.FromSubscription(_subscriptionId)
        .Value
    );

    _employee = subscriptionQuerySession
      .Query<Employee>()
      .FirstOrDefault(e => e.Email == _identityuser.Email);

    var users = globalQuerySession
      .Query<ActiveUser>()
      .ToList();

    _subscriptionUser = globalQuerySession
      .Query<ActiveUser>()
      .FirstOrDefault(u => u.Email == _identityuser.Email);


  }

  [Fact]
  public void should_register_user_in_identity()
  {
    _identityuser.ShouldNotBeNull();
    _identityuser.FirstName.ShouldBe(Nive.EmmaJohnson.FirstName);
  }

  [Fact]
  public void should_register_subscription_user()
  {
    _subscriptionUser.ShouldNotBeNull();
  }

  [Fact]
  public void should_set_up_subscription()
  {
    _subscription.ShouldNotBeNull();
    _subscription.CompanyName.ShouldBe(Nive.CompanyName);
  }

  [Fact]
  public void should_set_up_organization()
  {
    _organization.ShouldNotBeNull();
    _organization.CompanyName.ShouldBe(Nive.CompanyName);
  }

  [Fact]
  public void should_set_up_employee()
  {
    _employee.ShouldNotBeNull();
    _employee.Firstname.ShouldBe(Nive.EmmaJohnson.FirstName);
    _employee.Lastname.ShouldBe(Nive.EmmaJohnson.LastName);
  }

  public async Task DisposeAsync()
  {
    await _testServices.DropSubscriptionTestDatabase(_subscriptionId);
    await _testServices.DropGlobalTestDatabase();
    await _testServices.DropFreeUsersTestDatabase();
    await _host.DisposeAsync();
  }
}
