namespace ViteAspNetReact.Features;

public abstract class User
{
  public abstract Guid Id { get; set; }
  public string Username { get; set; } = null!;
  public string Email { get; protected init; } = null!;
  public string FirstName { get; protected init; } = null!;
  public string LastName { get; protected init; } = null!;
  public string Initials { get; protected init; } = null!;
  public Guid SubscriptionId { get; protected init; }
  public string Sub { get; protected init; } = null!;
  public string Fullname => $"{FirstName} {LastName}";
}
