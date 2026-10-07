using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogQuality.Data.Models;

//using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogQuality.Data.Repositories;

public class CatalogQualityDbContext : DbContextBase
{
    public CatalogQualityDbContext(DbContextOptions<CatalogQualityDbContext> options)
        : base(options)
    {
    }

    protected CatalogQualityDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QualityDataEntity>().ToTable("QualityData").HasKey(x => x.Id);
        modelBuilder.Entity<QualityDataEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogQuality.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogQuality.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogQuality.Data.SqlServer"));
                break;
        }
    }
}
