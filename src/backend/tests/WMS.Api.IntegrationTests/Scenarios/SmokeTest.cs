using Npgsql;
using WMS.Api.IntegrationTests.Fixtures;

namespace WMS.Api.IntegrationTests.Scenarios;

public class SmokeTest
{
    [Fact]
    public async Task PgConnect_ShouldSucceed()
    {
        var connString = SharedPostgreSqlFixture.GetAdminConnStr();
        connString.Should().NotBeNullOrEmpty();

        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        conn.State.Should().Be(ConnectionState.Open);
    }
}
