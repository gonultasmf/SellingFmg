using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace CatalogService.Infrastructure;

public static class HostExtensions
{
	public static IApplicationBuilder MigrateDbContext<TContext>(this IApplicationBuilder builder, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
	{
		using (var scope = builder.ApplicationServices.CreateScope())
		{
			var services = scope.ServiceProvider;
			var logger = services.GetRequiredService<ILogger<TContext>>();
			var context = services.GetService<TContext>();

			try
			{
				logger.LogInformation("Migration database associated with context {DbContextName}", typeof(TContext).Name);

				var retry = Policy.Handle<SqlException>()
					.WaitAndRetry(new TimeSpan[]
					{
						TimeSpan.FromSeconds(3),
						TimeSpan.FromSeconds(5),
						TimeSpan.FromSeconds(8)
					}); // buradaki yapı sqlexception hata alındığında ilk başta 3 saniye bekleyip ardından tekrar deneyip hata alırsa 5 ve ardında 8 saniye bekleyerek tekrar bağlanmaya çalışacak retry yani.

				retry.Execute(() => InvokeSeeder(seeder, context, services));

				logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);

            }
			catch (Exception ex)
			{
                logger.LogError("An error occured while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }
		}

		return builder;
	}

	private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services) where TContext : DbContext
	{
		context.Database.EnsureCreated(); // db tarafında oluşturulmayan şema tablo vs vs veya db varsa onları create edecek
		context.Database.Migrate(); // update edilmeyen migrate'leri update edecek db'ye
		seeder(context, services);
	}
}
