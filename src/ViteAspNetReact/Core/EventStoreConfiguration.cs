using Marten;
using Marten.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weasel.Core;

namespace ViteAspNetReact.Core;

// Implement IRetryPolicy interface

public class EventStoreConfiguration
{
  private const string DefaultSchema = Constants.ApplicationPrefix;

  public string ConnectionString { get; set; } = default!;

  public string WriteModelSchema { get; set; } = DefaultSchema;
  public string ReadModelSchema { get; set; } = DefaultSchema;

  public bool ShouldRecreateDatabase { get; set; }

  // public bool UseMetadata = true;
  public string DefaultEventstoreId { get; set; } = Constants.DefaultEventStore;
}

public static class MartenConfigExtensions
{
  private const string DefaultConfigKey = "EventStore";

  public static IServiceCollection AddMarten(
    this IServiceCollection services,
    IConfiguration config,
    Action<StoreOptions>? configureGlobalStoreOptions = null,
    Action<StoreOptions>? configureSubscriptionStoreOptions = null,
    Action<StoreOptions>? configureFreeUsersStoreOptions = null,
    string globalConfigKey = DefaultConfigKey,
    string freeUsersConfigKey = DefaultConfigKey,
    string subscriptionConfigKey = DefaultConfigKey
  )
  {
    var globalStoreConfigSection = config.GetSection(globalConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    var subscriptionConfigSection = config.GetSection(subscriptionConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    var freeUsersConfigSection = config.GetSection(freeUsersConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    services.AddMartenStore<IGlobalStore>(
      _ => SetGlobalStoreOptions(globalStoreConfigSection, configureGlobalStoreOptions)
    );

    services
      .AddSingleton(subscriptionConfigSection)
      .AddScoped<IIdGenerator, MartenIdGenerator>()
      .AddMartenStore<ISubscribersStore>(
        _ => SetSubscriberStoreOptions(
          subscriptionConfigSection,
          configureSubscriptionStoreOptions
        )
      );

    services.AddMartenStore<IFreeUsersStore>(
      _ =>
        SetFreeUsersStoreOptions(freeUsersConfigSection, configureFreeUsersStoreOptions)
    );

    return services;
  }

  private static StoreOptions SetGlobalStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var globalStoreOptions = new StoreOptions();
    globalStoreOptions.Connection(eventStoreConfiguration.ConnectionString);

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    globalStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    globalStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;
    
    globalStoreOptions.UseDefaultSerialization(
      EnumStorage.AsString,
      nonPublicMembersStorage: NonPublicMembersStorage.All
    );
    
    configureOptions?.Invoke(globalStoreOptions);

    return globalStoreOptions;
  }

  private static StoreOptions SetSubscriberStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var subscriberStoreOptions = new StoreOptions();
    // configure tenancy for database per tenant
    subscriberStoreOptions.MultiTenantedWithSingleServer(eventStoreConfiguration.ConnectionString);
    
    subscriberStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    subscriberStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    subscriberStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;

    subscriberStoreOptions.UseDefaultSerialization(
      EnumStorage.AsString,
      nonPublicMembersStorage: NonPublicMembersStorage.All
    );

    configureOptions?.Invoke(subscriberStoreOptions);

    return subscriberStoreOptions;
  }

  private static StoreOptions SetFreeUsersStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var freeUsersStoreOptions = new StoreOptions();
    freeUsersStoreOptions.Connection(eventStoreConfiguration.ConnectionString);
    
    // configure tenancy for tenantId column in the same database
    freeUsersStoreOptions.Policies.ForAllDocuments(x => x.TenancyStyle = TenancyStyle.Conjoined);
    freeUsersStoreOptions.Events.TenancyStyle = TenancyStyle.Conjoined;
    freeUsersStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    freeUsersStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    freeUsersStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;

    freeUsersStoreOptions.UseDefaultSerialization(
      EnumStorage.AsString,
      nonPublicMembersStorage: NonPublicMembersStorage.All
    );

    configureOptions?.Invoke(freeUsersStoreOptions);

    return freeUsersStoreOptions;
  }

  public static IServiceCollection AddTracing(
    this IServiceCollection services
  )
  {
    services.TryAddSingleton<ICorrelationIdFactory, GuidCorrelationIdFactory>();
    services.TryAddSingleton<ICausationIdFactory, GuidCausationIdFactory>();
    services.TryAddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
    services.TryAddScoped<ICausationIdProvider, CausationIdProvider>();
    services.TryAddScoped<ITraceMetadataProvider, TraceMetadataProvider>();
    services.TryAddSingleton<ITracingScopeFactory, TracingScopeFactory>();

    services.TryAddScoped<Func<IServiceProvider, TraceMetadata?, TracingScope>>(
      sp =>
        (
            scopedServiceProvider,
            traceMetadata
          ) =>
          sp.GetRequiredService<ITracingScopeFactory>()
            .CreateTraceScope(
              scopedServiceProvider,
              traceMetadata
            )
    );

    services.TryAddScoped<Func<IServiceProvider, EventEnvelope?, TracingScope>>(
      sp =>
      (
        scopedServiceProvider,
        eventEnvelope
      ) => sp.GetRequiredService<ITracingScopeFactory>()
        .CreateTraceScope(
          scopedServiceProvider,
          eventEnvelope
        )
    );

    return services;
  }
}
