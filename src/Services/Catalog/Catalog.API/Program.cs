string Namespace = typeof(Startup).Namespace;
string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

var configuration = GetConfiguration();

Log.Logger = CreateSerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", AppName);
    var host = BuildHost(configuration, args);

    Log.Information("Applying migrations ({ApplicationContext})...", AppName);
    var webHost = host.Services.GetService<IWebHost>();
    webHost!.MigrateDbContext<ApplicationDbContext>((context, services) =>
    {
        var env = services.GetService<IWebHostEnvironment>();
        var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
        var settings = services.GetService<IOptions<AppSettings>>();

        new ApplicationDbContextSeed()
            .SeedAsync(context, env!, logger!, settings!)
            .Wait();
    });

    Log.Information("Starting web host ({ApplicationContext})...", AppName);
    host.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IHost BuildHost(IConfiguration configuration, string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .CaptureStartupErrors(false)
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory());
        })
        .Build();

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        .WriteTo.Http(
            requestUri: string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl,
            queueLimitBytes: null)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    var config = builder.Build();

    return builder.Build();
}

