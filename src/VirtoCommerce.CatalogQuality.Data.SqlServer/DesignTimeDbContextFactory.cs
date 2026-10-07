using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogQuality.Data.Repositories;

namespace VirtoCommerce.CatalogQuality.Data.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogQualityDbContext>
{
    public CatalogQualityDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CatalogQualityDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=(local);User=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseSqlServer(
            connectionString,
            options => options.MigrationsAssembly(typeof(SqlServerDataAssemblyMarker).Assembly.GetName().Name));

        return new CatalogQualityDbContext(builder.Options);
    }
}
