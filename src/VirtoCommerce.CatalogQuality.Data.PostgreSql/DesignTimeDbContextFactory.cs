using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogQuality.Data.Repositories;

namespace VirtoCommerce.CatalogQuality.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogQualityDbContext>
{
    public CatalogQualityDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CatalogQualityDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(PostgreSqlDataAssemblyMarker).Assembly.GetName().Name));

        return new CatalogQualityDbContext(builder.Options);
    }
}
