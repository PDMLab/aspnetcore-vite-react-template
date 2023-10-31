using Marten;
using ViteAspNetReact.Features;

namespace ViteAspNetReact.Core;

public interface IGlobalStore : IDocumentStore
{
}

public interface ISubscribersStore : IDocumentStore
{
}

public interface IFreeUsersStore : IDocumentStore
{
}

public static class EventStore
{
  public static string GetSubscriberEventStoreId(
    SubscriptionId tenantId
  )
  {
    return $"{Constants.ApplicationPrefix}_sub_{tenantId.Value.ToString().Replace("-", "_")}";
  }

  public static string GetDefaultEventStoreId(
    EventStoreConfiguration? configuration
  )
  {
    return string.IsNullOrEmpty(configuration?.DefaultEventstoreId)
      ? Constants.DefaultEventStore
      : configuration.DefaultEventstoreId;
  }
}
