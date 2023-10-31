using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PasswordGenerator;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features.PrincipalExtensions;
using ViteAspNetReact.Features.RegisterUser;
using static ViteAspNetReact.Features.GetUserEmailStore.GetUserEmailStoreHelper;

namespace ViteAspNetReact.Features.InviteUser;

[Authorize]
public class InviteUserController : Controller
{
  [HttpGet, Route("/organization/user-invite")]
  public IActionResult InviteUserForm()
  {
    var model = new InviteUser(
      null,
      null,
      new EmailInputModel(),
      new PhoneNumberInputModel()
    );
    return View("~/Features/InviteUser/InviteUser.cshtml", model);
  }

  [HttpPost, Route("/organization/user-invite")]
  public async Task<IActionResult> InviteUser(
    [FromServices] UserManager<AppUser> userManager,
    [FromServices] IUserStore<AppUser> userStore,
    [FromServices] IGlobalStore globalStore,
    [FromServices] EventStoreConfiguration storeConfiguration,
    [FromServices] IEmailSender emailSender,
    [FromForm] InviteUser command,
    CancellationToken token
  )
  {
    if (!ModelState.IsValid)
    {
      return PartialView("~/Features/InviteUser/_InviteUserForm.cshtml", command);
    }

    var inviteUserHandler = new InviteUserHandler(
      userManager,
      userStore,
      globalStore,
      storeConfiguration,
      emailSender
    );

    await using var session = globalStore.QuerySession(
      EventStore.GetDefaultEventStoreId(storeConfiguration)
    );
    var inviter = session.Query<Features.User>()
      .FirstOrDefault(u => u.Sub == User.GetSub());

    await inviteUserHandler.Handle(
      command,
      inviter,
      token
    );

    return View("~/Features/InviteUser/_InviteUserForm.cshtml", command);
  }
}

public class InviteUserHandler
{
  private readonly UserManager<AppUser> _userManager;
  private readonly IUserStore<AppUser> _userStore;
  private readonly IGlobalStore _globalStore;
  private readonly EventStoreConfiguration _storeConfiguration;
  private readonly IEmailSender _emailSender;

  public InviteUserHandler(
    UserManager<AppUser> userManager,
    IUserStore<AppUser> userStore,
    IGlobalStore globalStore,
    EventStoreConfiguration storeConfiguration,
    IEmailSender emailSender
  )
  {
    _userManager = userManager;
    _userStore = userStore;
    _globalStore = globalStore;
    _storeConfiguration = storeConfiguration;
    _emailSender = emailSender;
  }

  public async Task Handle(
    InviteUser inviteUser,
    User inviter,
    CancellationToken token
  )
  {
    var (firstName, lastName, email, phone) = inviteUser;
    var userId = new UserId(Guid.NewGuid());
    var tenantId = inviter.SubscriptionId;

    var emailStore = GetEmailStore(_userManager, _userStore);


    var user = new AppUser
    {
      FirstName = firstName,
      LastName = lastName
    };
    await _userStore.SetUserNameAsync(
      user,
      email.Value,
      CancellationToken.None
    );
    await emailStore.SetEmailAsync(
      user,
      email.Value,
      CancellationToken.None
    );

    var password = new Password(
      true,
      true,
      true,
      true,
      16
    ).Next();
    var result = await _userManager.CreateAsync(
      user,
      password
    );

    if (result.Succeeded)
    {
      var invitedById = Guid.Parse(inviter.Sub);
      await _userManager.SetLockoutEnabledAsync(
        user,
        false
      );
      await _userManager.AddClaimAsync(
        user,
        new Claim(
          "tenantId",
          tenantId.ToString()
        )
      );
      var sub = user.Id;

      var userInvited = new UserInvited(
        userId,
        email.Value,
        firstName,
        lastName,
        tenantId.ToString(),
        sub,
        invitedById
      );

      var globalEventSoreId = EventStore.GetDefaultEventStoreId(_storeConfiguration);
      await using var session = _globalStore.LightweightSession(globalEventSoreId);

      session.Events.StartStream(userId.Value, userInvited);
      await session.SaveChangesAsync(token);

      var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
      await _emailSender.SendEmailAsync(user.Email, "Confirm your email", code);
    }
    else
    {
      // ReSharper disable once RedundantAssignment
      // ReSharper disable once EntityNameCapturedOnly.Local
      var errorDescriber = new IdentityErrorDescriber();
      if (result.Errors.Any(
            e => e.Code == nameof(errorDescriber.DuplicateUserName)
          ))
      {
        throw new UserAlreadyExistsException();
      }

      throw new ApplicationException(
        result.Errors.First()
          .Description
      );
    }
  }
}

public class EmailInputModel
{
  [Display(Name = "E-Mail")] public string Value { get; set; } = null!;
  public string Label { get; set; } = null!;
}

public class PhoneNumberInputModel
{
  public void Deconstruct(
    out string label,
    out string countryCode,
    out string areaCode,
    out string baseNumber,
    out string extension
  )
  {
    label = Label;
    countryCode = CountryCode;
    areaCode = AreaCode;
    baseNumber = BaseNumber;
    extension = Extension;
  }

  public string Label { get; set; } = null!;

  [Display(Name = "Int. Vorw.")] public string CountryCode { get; set; } = null!;

  [Display(Name = "Orts-Vorw.")] public string AreaCode { get; set; } = null!;

  [Display(Name = "Basis-Nr.")] public string BaseNumber { get; set; } = null!;

  [Display(Name = "Durchwahl")] public string Extension { get; set; } = null!;
}

public record InviteUser(
  [Display(Name = "Vorname")] [Required] string? FirstName,
  [Display(Name = "Nachname")]
  [Required]
  string? LastName,
  EmailInputModel Email,
  PhoneNumberInputModel Phone
);

public record UserInvited(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string TenantId,
  string Sub,
  Guid InvitedBy
);
