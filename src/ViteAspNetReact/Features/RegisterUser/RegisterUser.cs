using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Easy_Password_Validator;
using Easy_Password_Validator.Models;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features.AcceptUserInvitation;
using ViteAspNetReact.Features.RegisterEmployee;
using ViteAspNetReact.Features.SetCompanyNameFromUserRegistration;
using ViteAspNetReact.Features.Subscribe;
using StoreOptions = Marten.StoreOptions;
using static ViteAspNetReact.Features.GetUserEmailStore.GetUserEmailStoreHelper;

namespace ViteAspNetReact.Features.RegisterUser;

[AllowAnonymous]
public class RegisterUserController : Controller
{
  private readonly ILogger _logger;
  private readonly IIdGenerator _idGenerator;

  public RegisterUserController(
    ILogger logger,
    IIdGenerator idGenerator
  )
  {
    _logger = logger;
    _idGenerator = idGenerator;
  }

  [HttpGet, Route("/register-user")]
  public IActionResult RegisterUser() => View("~/Features/RegisterUser/RegisterUser.cshtml" );

  private IActionResult GenericErrorResult()
  {
    ModelState.AddModelError(
      "Generic",
      "Beim Speichern ist ein Fehler aufgetreten. Bitte versuchen Sie es erneut."
    );
    return View("~/Features/RegisterUser/_RegistrationForm.cshtml");
  }

  [HttpPost, Route("/register-user")]
  public async Task<IActionResult> RegisterUser(
    [FromServices] IUserStore<AppUser> userStore,
    [FromServices] UserManager<AppUser> userManager,
    [FromServices] IGlobalStore globalStore,
    [FromServices] ISubscribersStore subscribersStore,
    [FromServices] EventStoreConfiguration storeConfiguration,
    [FromForm] Registration registration,
    CancellationToken token
  )
  {
    try
    {
      if (!ModelState.IsValid)
      {
        return PartialView(
          "~/Features/RegisterUser/_RegistrationForm.cshtml",
          registration
        );
      }

      var emailStore = GetEmailStore(userManager, userStore);


      try
      {
        var handler = new RegisterUserHandler(
          userStore,
          userManager,
          globalStore,
          subscribersStore,
          storeConfiguration,
          _idGenerator,
          emailStore
        );

        await handler.Handle(registration, token);

        return PartialView("~/Features/RegisterUser/_RegistrationSucceeded.cshtml");
      }
      catch (Exception exception)
      {
        var result = exception switch
        {
          UserAlreadyExistsException => HandleUserAlreadyExistsResult(),
          _ => GenericErrorResult()
        };

        return result;
      }
    }
    catch (Exception exception)
    {
      _logger.LogCritical(
        exception,
        "Unexpected Exception during user registration {Message}",
        exception.Message
      );
      return GenericErrorResult();
    }
  }

  private IActionResult HandleUserAlreadyExistsResult()
  {
    ModelState.AddModelError(
      "Email",
      "Benutzer existiert bereits"
    );
    return View("~/Features/RegisterUser/_RegistrationForm.cshtml");
  }

  [HttpPost]
  [Route("/api/password-validation")]
  public IActionResult PasswordValidation(
    [FromForm] PasswordValidation passwordValidation
  )
  {
    if (string.IsNullOrWhiteSpace(passwordValidation.Password))
    {
      return PartialView(
        "~/Features/RegisterUser/_PasswordStrength.cshtml",
        new PasswordValidation
        {
          PasswordScore = null
        }
      );
    }

    var validator = new PasswordValidatorService(new PasswordRequirements());
    validator.TestAndScore(passwordValidation.Password);
    return PartialView(
      "~/Features/RegisterUser/_PasswordStrength.cshtml",
      new PasswordValidation
      {
        PasswordScore = validator.Score
      }
    );
  }
}

public class RegisterUserHandler
{
  private readonly IUserStore<AppUser> _userStore;
  private readonly UserManager<AppUser> _userManager;
  private readonly IGlobalStore _globalStore;
  private readonly ISubscribersStore _subscribersStore;
  private readonly EventStoreConfiguration _storeConfiguration;
  private readonly IIdGenerator _idGenerator;
  private readonly IUserEmailStore<AppUser> _emailStore;

  public RegisterUserHandler(
    IUserStore<AppUser> userStore,
    UserManager<AppUser> userManager,
    IGlobalStore globalStore,
    ISubscribersStore subscribersStore,
    EventStoreConfiguration storeConfiguration,
    IIdGenerator idGenerator,
    IUserEmailStore<AppUser> emailStore
  )
  {
    _userStore = userStore;
    _userManager = userManager;
    _globalStore = globalStore;
    _subscribersStore = subscribersStore;
    _storeConfiguration = storeConfiguration;
    _idGenerator = idGenerator;
    _emailStore = emailStore;
  }

  public async Task<SubscriptionId> Handle(
    Registration registration,
    CancellationToken token
  )
  {
    var companyName = registration.CompanyName!;
    var email = registration.Email!;
    var firstName = registration.FirstName!;
    var lastName = registration.LastName!;

    var tenantId = _idGenerator.New();
    var tenantIdString = tenantId.ToString();


    var password = registration.Password!;

    var user = new AppUser
    {
      FirstName = firstName,
      LastName = lastName
    };
    await _userStore.SetUserNameAsync(
      user,
      email,
      CancellationToken.None
    );
    await _emailStore.SetEmailAsync(
      user,
      email,
      CancellationToken.None
    );
    var result = await _userManager.CreateAsync(
      user,
      password
    );

    if (result.Succeeded)
    {
      await _userManager.AddClaimAsync(
        user,
        new Claim(
          "tenantId",
          tenantIdString
        )
      );
      var userId = new UserId(Guid.NewGuid());
      var employeeId = new EmployeeId(Guid.NewGuid());
      var subscriptionId = new SubscriptionId(tenantId);
      var organizationId = OrganizationId.FromSubscription(subscriptionId);

      var sub = user.Id;


      var userRegistered = new UserRegistered(
        userId,
        email,
        firstName,
        lastName,
        companyName,
        tenantIdString,
        sub
      );
      var registrationConfirmed =
        new RegistrationConfirmed(
          userId,
          email,
          firstName,
          lastName,
          companyName,
          tenantIdString,
          sub,
          DateTimeOffset.Now
        );

      var globalEventStoreId = EventStore.GetDefaultEventStoreId(_storeConfiguration);
      var subscriptionEventStoreId = EventStore.GetSubscriberEventStoreId(subscriptionId);

      // global store -> subscription stream
      var subscribed = new Subscribed(
        companyName,
        email,
        DateTimeOffset.Now,
        subscriptionId
      );

      // tenant store -> employee stream
      var employeeRegistered = new EmployeeRegistered(
        firstName,
        lastName,
        sub,
        email
      );

      // tenant store -> organization stream
      var companyNameSetFromUserRegistration = new CompanyNameSetFromUserRegistration(companyName);

      await using var globalSession = _globalStore.LightweightSession(globalEventStoreId);
      await using var subscriptionSession = _subscribersStore.LightweightSession(subscriptionEventStoreId);

      var userEvents = new object[] { userRegistered, registrationConfirmed };
      var subscriptionEvents = new object[] { subscribed };
      var employeeEvents = new object[] { employeeRegistered };
      var organizationEvents = new object[] { companyNameSetFromUserRegistration };

      globalSession.Events.StartStream(userId.Value, userEvents);
      globalSession.Events.StartStream(subscriptionId.Value, subscriptionEvents);

      await globalSession.SaveChangesAsync(token);

      subscriptionSession.Events.StartStream(employeeId.Value, employeeEvents);
      subscriptionSession.Events.StartStream(organizationId.Value, organizationEvents);

      await subscriptionSession.SaveChangesAsync(token);

      return subscriptionId;
    }
    else
    {
      // ReSharper disable once RedundantAssignment
      // ReSharper disable once EntityNameCapturedOnly.Local
      var errorDescriber = new IdentityErrorDescriber();
      if (result.Errors.Any(error => error.Code == nameof(errorDescriber.DuplicateUserName)))
      {
        throw new UserAlreadyExistsException();
      }

      throw new ApplicationException();
    }
  }
}

public class RegistrationModel
{
  [BindProperty] public Registration? Input { get; set; }
}

public class Registration
{
  [Required(ErrorMessage = "Vorname ist ein Pflichtfeld.")]
  [Display(Name = "Vorname")]
  public string? FirstName { get; set; }

  [Required(ErrorMessage = "Nachname ist ein Pflichtfeld.")]
  [Display(Name = "Nachname")]
  public string? LastName { get; set; }

  [Required(ErrorMessage = "Firma ist ein Pflichtfeld.")]
  [Display(Name = "Firma")]
  public string? CompanyName { get; set; }

  [Required(ErrorMessage = "Passwort ist ein Pflichtfeld.")]
  [Display(Name = "Passwort")]
  public string? Password { get; set; }

  [Required(ErrorMessage = "E-Mail ist ein Pflichtfeld.")]
  public string? Email { get; set; }

  [Range(
    typeof(bool),
    "true",
    "true",
    ErrorMessage = "Ihre Zustimmung zu den Bedingungen ist erforderlich."
  )]
  public bool TermsAccepted { get; set; }
}

public class PasswordValidation
{
  public int? PasswordScore { get; set; }
  public string? Password { get; set; }
}

public record UserRegistered(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string CompanyName,
  string TenantId,
  string Sub
);

public record RegistrationConfirmed(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string CompanyName,
  string TenantId,
  string Sub,
  DateTimeOffset On
);

public class RegisteredUser : User
{
  private RegisteredUser(
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
    Sub = sub;
    SubscriptionId = subscriptionId;
  }

  public static RegisteredUser Create(
    UserRegistered registered
  )
  {
    var initials = $"{registered.FirstName[..1]}{registered.LastName[..1]}";

    return new RegisteredUser(
      registered.Email,
      registered.FirstName,
      registered.LastName,
      initials,
      registered.Sub,
      Guid.Parse(
        registered.TenantId
      )
    );
  }

  internal bool ShouldDelete(
    RegistrationConfirmed registrationConfirmed
  ) => true;

  public override Guid Id { get; set; }
}

public class ActiveUser : User
{
  public DateTimeOffset RegistrationConfirmedOn { get; }

  private ActiveUser(
    Guid id,
    string email,
    string firstName,
    string lastName,
    string initials,
    string sub,
    Guid subscriptionId,
    DateTimeOffset registrationConfirmedOn
  )
  {
    RegistrationConfirmedOn = registrationConfirmedOn;
    Email = email;
    FirstName = firstName;
    LastName = lastName;
    Initials = initials;
    Sub = sub;
    SubscriptionId = subscriptionId;
    Id = id;
  }

  public static ActiveUser Create(
    RegistrationConfirmed confirmed
  )
  {
    var initials = $"{confirmed.FirstName[..1]}{confirmed.LastName[..1]}";

    return new ActiveUser(
      confirmed.UserId.Value,
      confirmed.Email,
      confirmed.FirstName,
      confirmed.LastName,
      initials,
      confirmed.Sub,
      Guid.Parse(
        confirmed.TenantId
      ),
      confirmed.On
    );
  }

  public static ActiveUser Create(
    UserInvitationAccepted accepted
  )
  {
    var initials = $"{accepted.FirstName[..1]}{accepted.LastName[..1]}";
    return new ActiveUser(
      accepted.UserId.Value,
      accepted.Email,
      accepted.FirstName,
      accepted.LastName,
      initials,
      accepted.Sub,
      Guid.Parse(
        accepted.TenantId
      ),
      accepted.On
    );
  }

  public override Guid Id { get; set; }
}

public static class UserProjectionConfiguration
{
  public static void UseRegisteredUserProjections(
    this StoreOptions options
  )
  {
    options.Schema
      .For<User>()
      .AddSubClass<RegisteredUser>()
      .AddSubClass<ActiveUser>();

    options.Projections.Snapshot<RegisteredUser>(
      SnapshotLifecycle.Inline
    );
    options.Projections.Snapshot<ActiveUser>(
      SnapshotLifecycle.Inline
    );
  }
}

public class UserAlreadyExistsException : ApplicationException
{
  public UserAlreadyExistsException() : base("User already exists")
  {
  }
}
