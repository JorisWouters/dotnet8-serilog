using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = new HostBuilder()
    .ConfigureAppConfiguration(builder =>
    {
        // read settings
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"AppSettings.{environment}.json", true, true)
            .AddEnvironmentVariables();

        // configure Serilog using configuration settings
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .CreateLogger();

        Log.Information("Starting up");

    })
    .ConfigureFunctionsWebApplication(builder =>
    {
        // manual config
        /*
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.Console()
            .CreateLogger();
        builder.Services.AddLogging(lb => lb.AddSerilog(Log.Logger, true));
        */
    })
    .ConfigureServices(services =>
    {
        const string configFile = "appsettings.json";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var config = new ConfigurationBuilder()
            .AddJsonFile(configFile, false)
            .AddJsonFile($"appSettings.{environment}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        services.AddLogging();
        services.AddResponseCompression();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();