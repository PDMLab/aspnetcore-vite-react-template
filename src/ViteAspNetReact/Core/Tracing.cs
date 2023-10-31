namespace ViteAspNetReact.Core;

public record CorrelationId(
  string Value
)
{
  public const string LoggerScopeKey = "Correlation-ID";
}

public interface ICorrelationIdFactory
{
  CorrelationId New();
}

public class GuidCorrelationIdFactory : ICorrelationIdFactory
{
  public CorrelationId New() => new(
    Guid.NewGuid()
      .ToString("N")
  );
}

public interface ICorrelationIdProvider
{
  void Set(
    CorrelationId correlationId
  );

  CorrelationId? Get();
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
  private CorrelationId? _value;

  public void Set(
    CorrelationId correlationId
  ) => _value = correlationId;

  public CorrelationId? Get() => _value;
}

public record TraceMetadata(
  CorrelationId? CorrelationId,
  CausationId? CausationId
);

public record CausationId(
  string Value
)
{
  public const string LoggerScopeKey = "Causation-ID";
}

public interface ICausationIdFactory
{
  CausationId New();
}

public class GuidCausationIdFactory : ICausationIdFactory
{
  public CausationId New() => new(
    Guid.NewGuid()
      .ToString("N")
  );
}

public interface ICausationIdProvider
{
  void Set(
    CausationId causationId
  );

  CausationId? Get();
}

public class CausationIdProvider : ICausationIdProvider
{
  private CausationId? _value;

  public void Set(
    CausationId causationId
  ) => _value = causationId;

  public CausationId? Get() => _value;
}

public interface ITraceMetadataProvider
{
  TraceMetadata? Get();
}

public class TraceMetadataProvider : ITraceMetadataProvider
{
  private readonly ICorrelationIdProvider _correlationIdProvider;
  private readonly ICausationIdProvider _causationIdProvider;

  public TraceMetadataProvider(
    ICorrelationIdProvider correlationIdProvider,
    ICausationIdProvider causationIdProvider
  )
  {
    _correlationIdProvider = correlationIdProvider;
    _causationIdProvider = causationIdProvider;
  }

  public TraceMetadata? Get()
  {
    var correlationId = _correlationIdProvider.Get();
    var causationId = _causationIdProvider.Get();

    if (correlationId == null && causationId == null) return null;

    return new TraceMetadata(
      correlationId,
      causationId
    );
  }
}

public class TracingScope : IDisposable
{
  public CorrelationId CorrelationId { get; }
  public CausationId CausationId { get; }
  private readonly IDisposable? _loggerScope;

  public TracingScope(
    IDisposable? loggerScope,
    CorrelationId correlationId,
    CausationId causationId
  )
  {
    _loggerScope = loggerScope;
    CorrelationId = correlationId;
    CausationId = causationId;
  }

  public void Dispose()
  {
    _loggerScope?.Dispose();
  }
}

public interface ITracingScopeFactory
{
  TracingScope CreateTraceScope(
    IServiceProvider serviceProvider,
    TraceMetadata? traceMetadata = null
  );
}

public class TracingScopeFactory : ITracingScopeFactory
{
  private readonly ILogger<TracingScopeFactory> _logger;
  private readonly ICorrelationIdFactory _correlationIdFactory;

  public TracingScopeFactory(
    ILogger<TracingScopeFactory> logger,
    ICorrelationIdFactory correlationIdFactory
  )
  {
    _logger = logger;
    _correlationIdFactory = correlationIdFactory;
  }

  public TracingScope CreateTraceScope(
    IServiceProvider serviceProvider,
    TraceMetadata? traceMetadata = null
  )
  {
    var correlationId = traceMetadata?.CorrelationId ?? _correlationIdFactory.New();

    var correlationIdProvider = serviceProvider.GetRequiredService<ICorrelationIdProvider>();
    correlationIdProvider.Set(correlationId);

    // if Causation Id was not provided, use Correlation Id
    var causationId = traceMetadata?.CausationId ?? new CausationId(correlationId.Value);

    var causationIdProvider = serviceProvider.GetRequiredService<ICausationIdProvider>();
    causationIdProvider.Set(causationId);

    var loggerScope = _logger.BeginScope(
      new Dictionary<string, object>
      {
        [CorrelationId.LoggerScopeKey] = correlationId.Value,
        [CausationId.LoggerScopeKey] = causationId.Value
      }
    );

    return new TracingScope(
      loggerScope,
      correlationId,
      causationId
    );
  }
}

public static class TraceScopeFactoryExtensions
{
  public static TracingScope CreateTraceScope(
    this ITracingScopeFactory tracingScopeFactory,
    IServiceProvider serviceProvider,
    EventEnvelope? eventEnvelope
  )
  {
    if (eventEnvelope == null) return tracingScopeFactory.CreateTraceScope(serviceProvider);

    var (_, eventMetadata) = eventEnvelope;

    var newCausationId = new CausationId(eventMetadata.EventId);

    return tracingScopeFactory.CreateTraceScope(
      serviceProvider,
      new TraceMetadata(
        eventMetadata.Trace?.CorrelationId,
        newCausationId
      )
    );
  }
}
