using Npgsql;

namespace ViteAspNetReact.IntegrationTests.Helpers;

public class PostgresAdministration
{
  private readonly string _connectionString;

  public PostgresAdministration(string connectionString)
  {
    _connectionString = connectionString;
  }

  public async Task CreateDatabase(string databaseName)
  {
    await using var connection = new NpgsqlConnection
    {
      ConnectionString = _connectionString
    };
    await connection.OpenAsync();
    var command = new NpgsqlCommand(
      $"CREATE DATABASE {databaseName}",
      connection
    );
    await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();
  }

  public async Task DropDatabase(string databaseName)
  {
    await using var connection = new NpgsqlConnection
    {
      ConnectionString = _connectionString
    };
    await connection.OpenAsync();
    var command = new NpgsqlCommand(
      $"DROP DATABASE IF EXISTS {databaseName} WITH (FORCE);",
      connection
    );
    await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();
  }

  public async Task<bool> EnsureDatabaseExists(
    string databaseName
  )
  {
    await using var connection = new NpgsqlConnection();
    connection.ConnectionString = _connectionString;

    await connection.OpenAsync();
    var command = new NpgsqlCommand(
      $"SELECT 1 FROM pg_database WHERE datname LIKE '{databaseName}'",
      connection
    );
    
    var result = await command.ExecuteScalarAsync();
    await connection.CloseAsync();
    
    return result != null;
  }
}
