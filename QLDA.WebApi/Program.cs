// Parse --provider argument BEFORE CreateBuilder (ASP.NET Core may consume CLI switches)
var provider = "sqlserver";
var providerIndex = Array.IndexOf(args, "--provider");
if (providerIndex >= 0 && providerIndex + 1 < args.Length)
    provider = args[providerIndex + 1].ToLowerInvariant();
var useSqlite = provider == "sqlite";

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var appSettings = new AppSettings();
configuration.Bind(appSettings);

builder.Host.ConfigureWebApiHost(configuration);

builder.Services.AddWebApiServices(configuration, appSettings, builder.Environment, useSqlite);

var app = builder.Build();

// Auto-create SQLite DB if using SQLite provider
if (useSqlite)
    app.EnsureCreatedAppDb();

app.UseWebApiConfiguration(appSettings);

app.Run();

public partial class Program { }
