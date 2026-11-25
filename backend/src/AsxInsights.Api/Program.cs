using AsxInsights.Api.Startup;
using AsxInsights.Application.Scoring;
using AsxInsights.Application.Scoring.Strategies;
using AsxInsights.Application.Services;
using AsxInsights.Infrastructure;
using AsxInsights.Infrastructure.MarketData;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=asx-insights.db";
var marketData = builder.Configuration.GetSection("MarketData").Get<MarketDataOptions>() ?? new MarketDataOptions();
var scoring = builder.Configuration.GetSection("Scoring").Get<ScoringOptions>() ?? new ScoringOptions();

// Infrastructure: EF Core + repositories + market data provider
builder.Services.AddInfrastructure(connectionString, marketData);

// Application: scoring strategies are registered individually, so adding a factor is one line here.
builder.Services.AddSingleton(scoring);
builder.Services.AddSingleton<IScoringStrategy, ValuationStrategy>();
builder.Services.AddSingleton<IScoringStrategy, TrendStrategy>();
builder.Services.AddSingleton<IScoringStrategy, MomentumStrategy>();
builder.Services.AddSingleton<IScoringStrategy, VolatilityStrategy>();
builder.Services.AddSingleton<IScoringStrategy, QualityStrategy>();
builder.Services.AddSingleton<IScoringStrategy, DividendStrategy>();
builder.Services.AddSingleton<ScoringEngine>();

builder.Services.AddScoped<MarketDataImportService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<StockQueryService>();

// Web
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("frontend");
app.MapControllers();
app.MapHealthChecks("/health");

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
