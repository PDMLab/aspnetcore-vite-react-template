using Marten;
using Marten.Events.Projections;
using ViteAspNetReact.Features.RegisterEmployee;

namespace ViteAspNetReact.Features.SetOrganizationAddress;

public static class RegisterOrganizationProjections
{
  public static void UseOrganizationProjections(
    this StoreOptions options
  )
  {
    options.Projections.Snapshot<Organization>(SnapshotLifecycle.Inline);
    options.Projections.Snapshot<Employee>(SnapshotLifecycle.Inline);
  }
}
