using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features.PrincipalExtensions;

namespace ViteAspNetReact.Features.SetSubscriptionAddress;

[Authorize]
public class SetSubscriptionAddressController : Controller
{
  [HttpGet, Route("/subscription")]
  public async Task<IActionResult> GetSubscription(
    [FromServices] ISubscribersStore subscribersStore,
    [FromServices] EventStoreConfiguration storeConfiguration,
    CancellationToken token
  )
  {
    var subscriptionId = User.GetSubscriptionId();

    await using var session =
      subscribersStore.LightweightSession(EventStore.GetDefaultEventStoreId(storeConfiguration));

    var subscription = session.Load<Subscription>(subscriptionId.Value);

    // TODO: 404

    return View(
      "~/Features/SetSubscriptionAddress/SetSubscriptionAddress.cshtml",
      new SetSubscriptionAddressModel(subscription)
    );
  }

  [HttpPut, Route("/subscription/address")]
  public async Task<IActionResult> SetSubscriptionAddress(
    [FromServices]ISubscribersStore subscribersStore,
    [FromServices]EventStoreConfiguration storeConfiguration,
    [FromForm] SetSubscriptionAddressModel model,
    CancellationToken token
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/SetSubscriptionAddress/SetSubscriptionAddressForm.cshtml", model);
    }

    var subscriptionId = User.GetSubscriptionId();

    var subscriptionAddressSet = new SubscriptionAddressSet(
      model.CompanyName,
      model.CompanyNameAddendum,
      model.AddressLine1,
      model.AddressLine2,
      model.ZipCode,
      model.City
    );

    await using var session =
      subscribersStore.LightweightSession(EventStore.GetDefaultEventStoreId(storeConfiguration));

    session.Events.Append(subscriptionId.Value, subscriptionAddressSet);
    
    await session.SaveChangesAsync(token);

    return View("~/Features/SetSubscriptionAddress/SetSubscriptionAddressForm.cshtml", model);
  }
}

public record SubscriptionAddressSet(
  string CompanyName,
  string? CompanyNameAddendum,
  string AddressLine1,
  string? AddressLine2,
  string ZipCode,
  string City
);

public record SetSubscriptionAddressModel
{
  public SetSubscriptionAddressModel()
  {
  }

  public SetSubscriptionAddressModel(
    Subscription subscription
  )
  {
    CompanyName = subscription.CompanyName;
    CompanyNameAddendum = subscription.CompanyNameAddendum;
    AddressLine1 = subscription.AddressLine1;
    AddressLine2 = subscription.AddressLine2;
    ZipCode = subscription.ZipCode;
    City = subscription.City;
  }

  [Required(ErrorMessage = "Firmenname ist ein Pflichtfeld")]
  [DisplayName("Firmenname")]
  public string? CompanyName { get; set; }

  [DisplayName("Zusatz Firmenname")] public string? CompanyNameAddendum { get; set; }

  [Required(ErrorMessage = "Straße ist ein Pflichtfeld")]
  [DisplayName("Straße und Hausnummer")]
  public string? AddressLine1 { get; set; }

  [DisplayName("Zusatz Adresse")] public string? AddressLine2 { get; set; }

  [DisplayName("PLZ")]
  [Required(ErrorMessage = "PLZ ist ein Pflichtfeld")]
  public string? ZipCode { get; set; }

  [DisplayName("Ort")]
  [Required(ErrorMessage = "Ort ist ein Pflichtfeld")]
  public string? City { get; set; }
}
