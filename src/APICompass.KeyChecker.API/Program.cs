using APICompass.KeyChecker.API.BackgroundServices;
using APICompass.KeyChecker.Core.Configuration;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Services;
using APICompass.KeyChecker.Infrastructure.Caching;
using APICompass.KeyChecker.Infrastructure.Http;
using APICompass.KeyChecker.Infrastructure.Queue;
using APICompass.KeyChecker.Validators.AI21;
using APICompass.KeyChecker.Validators.Anthropic;
using APICompass.KeyChecker.Validators.AWS;
using APICompass.KeyChecker.Validators.Azure;
using APICompass.KeyChecker.Validators.DeepSeek;
using APICompass.KeyChecker.Validators.ElevenLabs;
using APICompass.KeyChecker.Validators.MakerSuite;
using APICompass.KeyChecker.Validators.Mistral;
using APICompass.KeyChecker.Validators.OpenAI;
using APICompass.KeyChecker.Validators.OpenRouter;
using APICompass.KeyChecker.Validators.VertexAI;
using APICompass.KeyChecker.Validators.XAI;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/keychecker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting KeyChecker API...");

// Add configuration
builder.Services.Configure<ValidationSettings>(
    builder.Configuration.GetSection("ValidationSettings"));

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "KeyChecker API", 
        Version = "v1",
        Contact = new OpenApiContact()
        {
            Name = "Austin",
            Email = "15014915381z@gmail.com"
        },
        Description = "High-performance API for validating AI service API keys"
    });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add Redis (optional - falls back to memory cache if not configured)
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "KeyChecker:";
    });
    builder.Services.AddSingleton<IValidationCache, RedisCacheService>();
}
else
{
    builder.Services.AddSingleton<IValidationCache, MemoryCacheService>();
}

// Add background task queue
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// Add core services
builder.Services.AddSingleton<IKeyIdentifierService, KeyIdentifierService>();
builder.Services.AddScoped<IValidationOrchestrator, ValidationOrchestrator>();

// Configure HTTP clients with Polly policies
var httpClientNames = new[] { "OpenAI", "Anthropic", "AI21", "Mistral", "MakerSuite", "DeepSeek", "XAI", "OpenRouter", "ElevenLabs", "AWS", "Azure", "VertexAI" };

foreach (var name in httpClientNames)
{
    builder.Services.AddHttpClient(name, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "KeyChecker/1.0");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        MaxConnectionsPerServer = 500
    })
    .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy(5))
    .AddPolicyHandler(HttpClientPolicies.GetCircuitBreakerPolicy())
    .AddPolicyHandler(HttpClientPolicies.GetTimeoutPolicy(30));
}

// Register validators with semaphores
builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1500, 1500);
    return new OpenAIValidator(httpClientFactory.CreateClient("OpenAI"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1500, 1500);
    return new AnthropicValidator(httpClientFactory.CreateClient("Anthropic"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1000, 1000);
    return new AI21Validator(httpClientFactory.CreateClient("AI21"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1000, 1000);
    return new MistralValidator(httpClientFactory.CreateClient("Mistral"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(50, 50);
    return new MakerSuiteValidator(httpClientFactory.CreateClient("MakerSuite"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(50, 50);
    return new DeepSeekValidator(httpClientFactory.CreateClient("DeepSeek"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1500, 1500);
    return new XAIValidator(httpClientFactory.CreateClient("XAI"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1000, 1000);
    return new OpenRouterValidator(httpClientFactory.CreateClient("OpenRouter"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1000, 1000);
    return new ElevenLabsValidator(httpClientFactory.CreateClient("ElevenLabs"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(1000, 1000);
    return new AWSValidator(httpClientFactory.CreateClient("AWS"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(500, 500);
    return new AzureValidator(httpClientFactory.CreateClient("Azure"), cache, semaphore);
});

builder.Services.AddSingleton<IKeyValidator>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var cache = sp.GetRequiredService<IValidationCache>();
    var semaphore = new SemaphoreSlim(500, 500);
    return new VertexAIValidator(httpClientFactory.CreateClient("VertexAI"), cache, semaphore);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() ||  app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add Prometheus metrics endpoint
app.UseMetricServer();  // Exposes /metrics endpoint
app.UseHttpMetrics();   // Collects HTTP metrics

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

Log.Information("KeyChecker API started successfully on {urls}", builder.WebHost.GetSetting("urls"));

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
