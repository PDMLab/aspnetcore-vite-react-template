namespace ViteAspNetReact.Features.Subscribe;

public record Subscribed(
  string CompanyName,
  string RegisteredBy,
  DateTimeOffset RegisteredOn,
  SubscriptionId SubscriptionId
);


