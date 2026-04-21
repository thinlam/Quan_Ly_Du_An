var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Bind AppSettings
var appSettings = new AppSettings();
configuration.Bind(appSettings);

// Custom host config (của project cậu)
builder.Host.ConfigureWebApiHost(configuration);

// Add services
builder.Services.AddWebApiServices(configuration, appSettings, builder.Environment);

// 👉 BẮT BUỘC phải có Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 👉 CONFIG PIPELINE
app.UseSwagger();          // 🔥 QUAN TRỌNG
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger"; // truy cập /swagger
});

// config custom của project
app.UseWebApiConfiguration(appSettings);

app.Run();