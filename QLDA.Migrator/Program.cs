using System.Reflection;
using Polly;
using QLDA.Infrastructure;
using QLDA.Migrator.ConfigurationOptions;
using QLDA.Persistence;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

var appSettings = new AppSettings();
configuration.Bind(appSettings);

// Parse --provider argument (default: sqlserver)
var provider = "sqlserver";
var providerIndex = Array.IndexOf(args, "--provider");
if (providerIndex >= 0 && providerIndex + 1 < args.Length)
    provider = args[providerIndex + 1].ToLowerInvariant();

var useSqlite = provider == "sqlite";

services.Configure<AppSettings>(configuration);
services.AddDateTimeProvider();

if (useSqlite)
{
    var sqliteConn = appSettings.ConnectionStrings.SqliteConnection;
    if (string.IsNullOrEmpty(sqliteConn))
        throw new InvalidOperationException(
            "Missing ConnectionStrings:SqliteConnection in appsettings.json");
    services.AddPersistenceSqlite(sqliteConn);
}
else
{
    services.AddPersistence(appSettings.ConnectionStrings,
        Assembly.GetExecutingAssembly().GetName().Name);
}

var app = builder.Build();

if (useSqlite)
{
    app.EnsureCreatedAppDb();
    Console.WriteLine("SQLite database created successfully.");
}
else
{
    Policy.Handle<Exception>().WaitAndRetry(new[] {
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(20),
            TimeSpan.FromSeconds(30),
        })
        .Execute(() => { app.MigrateAppDb(); });
    Console.WriteLine("SQL Server migrations applied successfully.");
}

app.MapGet("/", () => $"QLDA Migrator — Provider: {provider}");
app.Run();
