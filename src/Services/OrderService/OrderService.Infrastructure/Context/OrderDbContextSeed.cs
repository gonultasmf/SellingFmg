using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain;
using Polly;

namespace OrderService.Infrastructure;

public class OrderDbContextSeed
{
    public async Task SeedAsync(OrderDbContext context, ILogger<OrderDbContextSeed> logger)
    {
        var policy = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider:
                retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attemp {retry} of {retry}");
                });

        var setupDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seeding", "Setup");

        await policy.ExecuteAsync(async() =>
        {
            await ProcessSeeding(context, setupDirPath, logger);
        });
    }

    private async Task ProcessSeeding(OrderDbContext context, string setupDirPath, ILogger<OrderDbContextSeed> logger)
    {
        using (context)
        {
            context.Database.EnsureCreated();
            context.Database.Migrate();
            if (!context.CardTypes.Any())
            {
                await context.CardTypes.AddRangeAsync(GetCardTypeFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }

            if (!context.PaymentMethods.Any())
            {
                await context.OrderStatus.AddRangeAsync(GetOrderStatusFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }
        }
        
    }

    private IEnumerable<OrderStatus> GetOrderStatusFromFile(string setupDirPath)
    {
        IEnumerable<OrderStatus> GetPreconfiguredPaymentMethod()
        {
            return Enumeration.GetAll<OrderStatus>();
        }

        string fileName = Path.Combine(setupDirPath, "OrderStatus.txt");

        if (!File.Exists(fileName))
        {
            return GetPreconfiguredPaymentMethod();
        }

        var fileContent = File.ReadAllLines(fileName);

        int id = 1;
        var list = fileContent.Select(x => new OrderStatus(id++, x)).Where(x => x is not null);

        return list ?? GetPreconfiguredPaymentMethod();
    }

    private IEnumerable<CardType> GetCardTypeFromFile(string setupDirPath)
    {
        IEnumerable<CardType> GetPreconfiguredCardType()
        {
            return Enumeration.GetAll<CardType>();
        }

        string fileName = Path.Combine(setupDirPath, "CardType.txt");

        if (!File.Exists(fileName))
        {
            return GetPreconfiguredCardType();
        }

        var fileContent = File.ReadAllLines(fileName);

        int id = 1;
        var list = fileContent.Select(x => new CardType(id++, x)).Where(x => x is not null);

        return list ?? GetPreconfiguredCardType();
    }
}
