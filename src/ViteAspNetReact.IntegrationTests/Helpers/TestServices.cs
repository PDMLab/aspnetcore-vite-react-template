using Alba;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features;
using Xunit.Abstractions;
using static ViteAspNetReact.IntegrationTests.Helpers.EventStoreHelpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ViteAspNetReact.IntegrationTests.Helpers;

public class TestConfiguration : Dictionary<string, string?>
{
  public IConfigurationRoot AsConfigurationRoot()
  {
    return new ConfigurationBuilder()
      .AddInMemoryCollection(this)
      .Build();
  }
}

public static class AlbaHostExtensions
{
  public static T? GetScopedService<T>(
    this IAlbaHost host
  )
  {
    var scope = host.Services.CreateScope();

    return scope.ServiceProvider.GetService<T>();
  }

  public static async Task<IAlbaHost> MigrateIdentityDatabase(
    this IAlbaHost host
  )
  {
    var scope = host.Services.CreateScope();

    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
    await context!.Database.MigrateAsync();

    return host;
  }
}

public static class TestConfigurationExtensions
{
  public static IWebHostBuilder SetTestConfiguration(
    this IWebHostBuilder builder,
    TestConfiguration configuration
  )
  {
    return builder.ConfigureAppConfiguration(
      (
        _,
        configurationBuilder
      ) => configurationBuilder.AddInMemoryCollection(configuration)
    );
  }
}

public class TestServices
{
  private readonly string _dbName;
  public string GlobalTestDbName { get; }
  private readonly PostgresAdministration _pgAdmin;
  public string FreeUsersTestDbName { get; }

  public TestServices()
  {
    _dbName = GetTestDbName();
    GlobalTestDbName = $"{_dbName}_global";
    FreeUsersTestDbName = $"{_dbName}_free_users";
    _pgAdmin = new PostgresAdministration(GetTestConnectionString());
  }

  public async Task<IHostBuilder> GetTestHostBuilder(
    ITestOutputHelper? testOutputHelper = null
  )
  {
    if (testOutputHelper is not null)
    {
      var configuration = await GetTestConfigurationRoot();

      Log.Logger = new LoggerConfiguration()
        // add the xunit test output sink to the serilog logger
        // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();

      var serilogLogger = Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();

      var dotnetILogger = new SerilogLoggerFactory(serilogLogger)
        .CreateLogger<Program>();

      var builder = ConfigureHost.GetHostBuilder(
        configuration,
        services =>
        {
          services.AddSingleton<ILogger>(dotnetILogger);
          services.UseTestEmailSender();
          services.UseTestEmailTemplateService();
        }
      );
      builder.UseSerilog(serilogLogger);

      return builder;
    }
    else
    {
      var configuration = await GetTestConfigurationRoot();

      var builder = ConfigureHost.GetHostBuilder(
        configuration,
        services =>
        {
          services.UseTestEmailSender();
          services.UseTestEmailTemplateService();
        }
      );

      return builder;
    }
  }

  public async Task<TestConfiguration> GetTestConfiguration()
  {
    await _pgAdmin.CreateDatabase(GlobalTestDbName);
    await _pgAdmin.CreateDatabase(FreeUsersTestDbName);
    var testConnectionString = GetTestConnectionString(GlobalTestDbName);
    var identityTestConnectionString = GetTestIdentityConnectionString(GlobalTestDbName);
    return new TestConfiguration
    {
      { "ConnectionStrings:Identity", identityTestConnectionString },
      { "EventStore:ConnectionString", testConnectionString },
      { "EventStore:WriteModelSchema", Constants.ApplicationPrefix },
      { "EventStore:ReadModelSchema", Constants.ApplicationPrefix },
      { "EventStore:DefaultEventstoreId", GlobalTestDbName }
    };
  }

  public async Task<IConfigurationRoot> GetTestConfigurationRoot()
  {
    var testConfiguration = await GetTestConfiguration();
    var configurationRoot = new ConfigurationBuilder()
      .AddInMemoryCollection(testConfiguration)
      .Build();

    return configurationRoot;
  }

  public async Task DropMainTestDatabase()
  {
    await _pgAdmin.DropDatabase(_dbName);
  }

  public async Task DropSubscriptionTestDatabase(
    SubscriptionId subscriptionId
  )
  {
    await _pgAdmin.DropDatabase(EventStore.GetSubscriberEventStoreId(subscriptionId));
  }

  public async Task<bool> EnsureGlobalTestDatabaseExists() => await _pgAdmin.EnsureDatabaseExists(GlobalTestDbName);

  public async Task<bool> EnsureFreeUsersTestDatabaseExists() =>
    await _pgAdmin.EnsureDatabaseExists(FreeUsersTestDbName);

  public async Task DropGlobalTestDatabase() => await _pgAdmin.DropDatabase(GlobalTestDbName);
  
  public async Task DropFreeUsersTestDatabase() => await _pgAdmin.DropDatabase(FreeUsersTestDbName);
  
  public async Task DropSubscriberTestDatabase(
    SubscriptionId subscriptionId
  ) => await _pgAdmin.DropDatabase(EventStore.GetSubscriberEventStoreId(subscriptionId));
}
