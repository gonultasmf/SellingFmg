using StackExchange.Redis;

namespace BasketService.Api;

public static class RedisRegistration
{
    public static ConnectionMultiplexer ConfigureRedis(this IServiceProvider services, IConfiguration configuration)
    {
        var redisConfig = ConfigurationOptions.Parse(configuration["RedisSettings:ConnectionString"], true);
        redisConfig.ResolveDns = true;

        return ConnectionMultiplexer.Connect(redisConfig);
    }
}
