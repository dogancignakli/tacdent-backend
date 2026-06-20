using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tacdent.Data.Context;

/// <summary>
/// Lets EF Core tooling (dotnet ef migrations/database) build a context without spinning up the API host.
/// The connection string is only used at design time; override it with the TACDENT_DB environment variable.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TacdentDbContext>
{
    public TacdentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TACDENT_DB")
            ?? "Host=localhost;Port=5432;Database=tacdent;Username=tacdent;Password=tacdent_dev";

        var options = new DbContextOptionsBuilder<TacdentDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TacdentDbContext(options);
    }
}
