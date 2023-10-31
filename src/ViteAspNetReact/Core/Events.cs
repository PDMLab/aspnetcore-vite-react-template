namespace ViteAspNetReact.Core;

public interface IEventHandler<in TEvent>
{
  Task Handle(
    TEvent @event,
    CancellationToken cancellationToken
  );
}

public record EventMetadata(
  string EventId,
  string TenantId,
  ulong StreamPosition,
  ulong LogPosition,
  TraceMetadata? Trace
);

public record EventEnvelope(
  object Data,
  EventMetadata Metadata
);

public record EventEnvelope<T>(
  T Data,
  EventMetadata Metadata
) : EventEnvelope(
  Data,
  Metadata
) where T : notnull
{
  public new T Data => (T)base.Data;
}
