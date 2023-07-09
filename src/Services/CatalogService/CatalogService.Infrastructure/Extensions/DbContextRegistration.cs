using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure;

public static class DbContextRegistration
{
    public static IServiceCollection ConfigureDbContext(this IServiceCollection services)
    {
        services.AddEntityFrameworkSqlServer()
            .AddDbContext<CatalogContext>(options =>
            {
                options.UseSqlServer(Configuration.ConnectionString,
                                     sqlServerOptionsAction: sqlOption =>
                                     {
                                         sqlOption.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                     });

            });

        return services;
    }
}
