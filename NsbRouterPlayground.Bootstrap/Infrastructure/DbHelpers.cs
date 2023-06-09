using System.Data;
using Microsoft.Data.SqlClient;

namespace NsbRouterPlayground.Bootstrap.Infrastructure;

public class DbHelpers
{
  private const string DatabaseNameToken = "#database#";
  private static readonly string CreateDatabaseIfMissing = $@"
IF (db_id(N'{DatabaseNameToken}') IS NULL)
  CREATE DATABASE [{DatabaseNameToken}]
";

  private const string SchemaNameToken = "#schema#";
  private static readonly string CreateSchemaIfMissing = $@"
IF NOT EXISTS
(
  SELECT 1 
  FROM [INFORMATION_SCHEMA].[SCHEMATA]
  WHERE [SCHEMA_NAME] = N'{SchemaNameToken}'
)
BEGIN

  /* The schema must be run in its own batch! */
  EXEC(N'CREATE SCHEMA [{SchemaNameToken}] AUTHORIZATION [dbo]');

END    
";

  public static async Task EnsureDatabaseExists(string connectionString, string? schemaName = "nsb")
  {
    var (cnnStr, dbName) = ProcessConnectionString(connectionString);
    using (var cn = new SqlConnection(cnnStr))
    {
      await cn.OpenAsync();
      using (var cmd = cn.CreateCommand())
      {
        cmd.CommandText = CreateDatabaseIfMissing.Replace(DatabaseNameToken, dbName);
        cmd.CommandType = CommandType.Text;

        await cmd.ExecuteNonQueryAsync();
      }
    }

    if (!string.IsNullOrWhiteSpace(schemaName))
    {
      await EnsureSchemaExists(connectionString, schemaName);
    }
  }

  #region Internals

  private static async Task EnsureSchemaExists(string cnnStr, string schemaName)
  {
    using (var cn = new SqlConnection(cnnStr))
    {
      await cn.OpenAsync();
      using (var cmd = cn.CreateCommand())
      {
        cmd.CommandText = CreateSchemaIfMissing.Replace(SchemaNameToken, schemaName);
        cmd.CommandType = CommandType.Text;

        await cmd.ExecuteNonQueryAsync();
      }
    }
  }

  private static (string connectToMaster, string databaseName) ProcessConnectionString(string connectionString)
  {
    var csb = new SqlConnectionStringBuilder(connectionString);

    var dbName = csb.InitialCatalog;
    csb.InitialCatalog = "master";

    return (csb.ConnectionString, dbName);
  }

  #endregion
}