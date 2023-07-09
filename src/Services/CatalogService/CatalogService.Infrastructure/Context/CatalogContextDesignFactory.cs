using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Infrastructure.Context;

public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer("Data Source=FMG;Initial Catalog=catalog;Persist Security Info=True;User ID=sa;Password=FsMg2544");

        return new CatalogContext(optionsBuilder.Options);
    }
}
