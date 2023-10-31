using Marten;
using Marten.Events.Projections;
using ViteAspNetReact.Features.SetSubscriptionAddress;
using ViteAspNetReact.Features.Subscribe;

namespace ViteAspNetReact.Features;

public static class SubscriptionsProjectionConfiguration
{
  public static void UseSubscriptionProjections(
    this StoreOptions options
  )
  {
    options.Projections.Snapshot<Subscription>(SnapshotLifecycle.Inline);
  }
}

public class Subscription
{
  private Subscription(
    string companyName,
    string registeredBy,
    DateTimeOffset registeredOn
  )
  {
    CompanyName = companyName;
    RegisteredBy = registeredBy;
    RegisteredOn = registeredOn;
  }

  // Create a new aggregate based on the initial
  // event type
  // ReSharper disable once UnusedMember.Global
  public static Subscription Create(Subscribed subscribed)
  {
    var (companyName, registeredBy, registeredOn, _) = subscribed;
    var tenant = new Subscription(
      companyName,
      registeredBy,
      registeredOn
    );
    return tenant;
  }

  // ReSharper disable once UnusedMember.Global
  public void Apply(SubscriptionAddressSet subscriptionAddressSet)
  {
    var (companyName, companyNameAddendum, addressLine1, addressLine2, zipCode, city) = subscriptionAddressSet;

    CompanyName = companyName;
    CompanyNameAddendum = companyNameAddendum;
    AddressLine1 = addressLine1;
    AddressLine2 = addressLine2;
    ZipCode = zipCode;
    City = city;
  }

  // ReSharper disable once UnusedMember.Global
  public Guid Id { get; set; }
  public string CompanyName { get; private set; }
  public string RegisteredBy { get; }
  public DateTimeOffset RegisteredOn { get; }
  public string? CompanyNameAddendum { get; private set; }
  public string? AddressLine1 { get; private set; }
  public string? AddressLine2 { get; private set; }
  public string? ZipCode { get; private set; }
  public string? City { get; private set; }
}
