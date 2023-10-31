using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features.PrincipalExtensions;
using ViteAspNetReact.Features.SetCompanyNameFromUserRegistration;

namespace ViteAspNetReact.Features.SetOrganizationAddress;

[Authorize]
public class SetOrganizationAddressController : Controller
{
  [HttpGet, Route("/organization")]
  public async Task<IActionResult> Index(
    [FromServices] ISubscribersStore store
  )
  {
    var subscriptionId = User.GetSubscriptionId();
    var organizationId = OrganizationId.FromSubscription(subscriptionId);
    await using var session = store.QuerySession(EventStore.GetSubscriberEventStoreId(subscriptionId));
    var organization = session.Load<Organization>(organizationId.Value);
    var model = new SetOrgAddress(organization);

    return View("~/Features/SetOrganizationAddress/SetOrganizationAddress.cshtml", model);
  }


  [HttpPut]
  [Route("/organization")]
  public async Task<IActionResult> SetOrganizationAddress(
    [FromForm] SetOrgAddress model,
    [FromServices] ISubscribersStore store
  )
  {
    if (!ModelState.IsValid)
    {
      return View(
        "~/Features/SetOrganizationAddress/_OrganizationAddressForm.cshtml",
        model
      );
    }

    var subscriptionId = User.GetSubscriptionId();

    await using var session =
      store.LightweightSession(EventStore.GetSubscriberEventStoreId(subscriptionId));

    var (companyName, companyNameAddendum, addressLine1, addressLine2, zipCode, city) = model;

    session.Events.Append(
      OrganizationId.FromSubscription(subscriptionId)
        .Value,
      new OrganizationAddressSet(
        companyName,
        companyNameAddendum,
        addressLine1,
        addressLine2,
        zipCode,
        city
      )
    );

    await session.SaveChangesAsync();

    return PartialView("~/Features/SetOrganizationAddress/_OrganizationAddressForm.cshtml", model);
  }
}

public class Organization
{
  // ReSharper disable once UnusedMember.Global
  public Guid Id { get; set; }
  public string CompanyName { get; private set; }
  public string? CompanyNameAddendum { get; private set; }
  public string? AddressLine1 { get; private set; }
  public string? AddressLine2 { get; private set; }
  public string? ZipCode { get; private set; }
  public string? City { get; private set; }

  private Organization(
    string companyName
  )
  {
    CompanyName = companyName;
  }

  // ReSharper disable once UnusedMember.Global
  public static Organization Create(
    CompanyNameSetFromUserRegistration nameSetFromUserRegistration
  )
  {
    var companyName = nameSetFromUserRegistration.CompanyName;
    return new Organization(companyName);
  }

  // ReSharper disable once UnusedMember.Global
  public void Apply(
    OrganizationAddressSet organizationAddressSet
  )
  {
    var (companyName, companyNameAddendum, addressLine1, addressLine2, zipCode, city) = organizationAddressSet;
    CompanyName = companyName;
    CompanyNameAddendum = companyNameAddendum;
    AddressLine1 = addressLine1;
    AddressLine2 = addressLine2;
    ZipCode = zipCode;
    City = city;
  }
}

public record OrganizationAddressSet(
  string CompanyName,
  string? CompanyNameAddendum,
  string AddressLine1,
  string? AddressLine2,
  string ZipCode,
  string City
);

public class SetOrgAddress
{
  public SetOrgAddress() : this(
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty
  )
  {
  }

  public SetOrgAddress(
    Organization organization
  ) :
    this(
      organization.CompanyName,
      organization.CompanyNameAddendum,
      organization.AddressLine1,
      organization.AddressLine2,
      organization.ZipCode,
      organization.City
    )
  {
  }

  public SetOrgAddress(
    string companyName,
    string? companyNameAddendum,
    string? addressLine1,
    string? addressLine2,
    string? zipCode,
    string? city
  )
  {
    CompanyName = companyName;
    CompanyNameAddendum = companyNameAddendum;
    AddressLine1 = addressLine1;
    AddressLine2 = addressLine2;
    ZipCode = zipCode;
    City = city;
  }

  [DisplayName("Firmenname")]
  [Required(ErrorMessage = "Firmenname ist ein Pflichtfeld")]
  public string CompanyName { get; init; }

  [DisplayName("Zusatz Firmenname")] public string? CompanyNameAddendum { get; init; }

  [DisplayName("Straße und Hausnummer")]
  [Required(ErrorMessage = "Straße ist ein Pflichtfeld")]
  public string? AddressLine1 { get; init; }

  [DisplayName("Zusatz Adresse")] public string? AddressLine2 { get; init; }

  [DisplayName("PLZ")]
  [Required(ErrorMessage = "PLZ ist ein Pflichtfeld")]
  public string? ZipCode { get; init; }

  [DisplayName("Ort")]
  [Required(ErrorMessage = "Ort ist ein Pflichtfeld")]
  public string? City { get; init; }

  public void Deconstruct(
    out string companyName,
    out string? companyNameAddendum,
    out string? addressLine1,
    out string? addressLine2,
    out string? zipCode,
    out string? city
  )
  {
    companyName = CompanyName;
    companyNameAddendum = CompanyNameAddendum;
    addressLine1 = AddressLine1;
    addressLine2 = AddressLine2;
    zipCode = ZipCode;
    city = City;
  }

}
