var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var appSettings = new AppSettings();
configuration.Bind(appSettings);

builder.Host.ConfigureWebApiHost(configuration);

builder.Services.AddWebApiServices(configuration, appSettings, builder.Environment);

var app = builder.Build();

app.UseWebApiConfiguration(appSettings);

app.Run();

public partial class Program { }