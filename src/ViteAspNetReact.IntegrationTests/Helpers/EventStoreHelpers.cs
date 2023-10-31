using Npgsql;
using ViteAspNetReact.Core;

namespace ViteAspNetReact.IntegrationTests.Helpers;

public class EventStoreHelpers
{
  public static string GetTestDbName()
  {
    Task
      .Delay(new Random().Next(50, 100))
      .Wait();

    var dbId = DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_fff");
    return $"{Constants.ApplicationPrefix}_{dbId}_{Guid.NewGuid().ToString()[..4]}";
  }

  public static string GetTestConnectionString()
  {
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
    {
      Pooling = false,
      Port = 5435,
      Host = "localhost",
      CommandTimeout = 20,
      Database = "postgres",
      Password = "123456",
      Username = "postgres"
    };
    var pgTestConnectionString = connectionStringBuilder.ToString();

    return pgTestConnectionString;
  }

  public static string GetTestConnectionString(
    string dbName
  )
  {
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
    {
      Pooling = false,
      Port = 5435,
      Host = "localhost",
      CommandTimeout = 20,
      Database = dbName,
      Password = "123456",
      Username = "postgres"
    };
    var pgTestConnectionString = $"{connectionStringBuilder};Include Error Detail=True";

    return pgTestConnectionString;
  }

  public static string GetTestIdentityConnectionString(
    string dbName
  ) =>
    GetTestConnectionString(
      dbName
    );
}
