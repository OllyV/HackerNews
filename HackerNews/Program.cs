using Hacker_News.ApiClients;
using Hacker_News.Helpers;
using Hacker_News.Interfaces;
using Hacker_News.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddHttpLogging(httpLoggingOptions =>
{
    httpLoggingOptions.LoggingFields =
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.ResponseStatusCode;
});

builder.Services.AddMemoryCache(options => options.SizeLimit = 500);
builder.Services.AddHttpClient<IHackerNewsApiClient, HackerNewsApiClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler((services, _) =>
    GetRetryPolicy(services.GetRequiredService<ILogger<HackerNewsApiClient>>()))
.AddPolicyHandler((services, _) =>
    GetCircuitBreakerPolicy(services.GetRequiredService<ILogger<HackerNewsApiClient>>()));

builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(cfg => { }, typeof(StoryProfile));

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(basePolicy => basePolicy.Expire(TimeSpan.FromSeconds(120)));
});

var app = builder.Build();
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseOutputCache();
app.Run();


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                logger.LogWarning(
                    outcome.Exception,
                    "Retry {RetryAttempt} after {Delay}s",
                    retryAttempt, timespan.TotalSeconds);
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(20),
            onBreak: (result, breakDelay) =>
            {
                logger.LogError(
                    result.Exception,
                    "Circuit opened for {BreakDelay}s",
                    breakDelay.TotalSeconds);
            },
            onReset: () => logger.LogInformation("Circuit closed. Service healthy again."));
}
