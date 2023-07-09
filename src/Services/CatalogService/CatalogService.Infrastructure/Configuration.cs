using Microsoft.Extensions.Configuration;

namespace CatalogService.Infrastructure;

public class Configuration
{
    public static string ConnectionString
    {
        get
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json")
               .Build();

            return configuration.GetConnectionString("SeelingFMG_Connection");
        }
    }
}
