using Npgsql;

namespace WMS.Api.IntegrationTests.Fixtures;

public class SharedPostgreSqlFixture
{
    private const string AdminConnStr = "Host=localhost;Port=5432;Database=wms_test;Username=postgres;Password=postgres";

    public static string GetAdminConnStr() => AdminConnStr;

    public static async Task<string> CreateTenantDatabaseAsync(string dbName)
    {
        await using var conn = new NpgsqlConnection(AdminConnStr);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName";
        cmd.Parameters.AddWithValue("dbName", dbName);
        var exists = await cmd.ExecuteScalarAsync();

        if (exists is null or 0)
        {
            await using var createCmd = conn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
            await createCmd.ExecuteNonQueryAsync();
        }

        return $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
    }

    public static async Task DropDatabaseAsync(string dbName)
    {
        await using var conn = new NpgsqlConnection(AdminConnStr);
        await conn.OpenAsync();

        var killSql = """
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = @dbName
            AND pid <> pg_backend_pid()
            """;
        await using var killCmd = conn.CreateCommand();
        killCmd.CommandText = killSql;
        killCmd.Parameters.AddWithValue("dbName", dbName);
        await killCmd.ExecuteNonQueryAsync();

        await using var dropCmd = conn.CreateCommand();
        dropCmd.CommandText = $"DROP DATABASE \"{dbName}\"";
        await dropCmd.ExecuteNonQueryAsync();
    }
}
