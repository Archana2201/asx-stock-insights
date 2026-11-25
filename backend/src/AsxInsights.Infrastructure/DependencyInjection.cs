using AsxInsights.Application.Abstractions;
using AsxInsights.Domain.Abstractions;
using AsxInsights.Infrastructure.MarketData;
using AsxInsights.Infrastructure.Persistence;
using AsxInsights.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AsxInsights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        MarketDataOptions marketData)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();
        services.AddScoped<IDataImportRepository, DataImportRepository>();

        services.AddSingleton(marketData);
        services.AddSingleton<IMarketDataProvider>(_ => MarketDataProviderFactory.Create(marketData));
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
