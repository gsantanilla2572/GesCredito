using System.Text.Json.Serialization;
using GesCredito.Infrastructure;
using GesCredito.Infrastructure.Data;
using GesCredito.Services;
using GesCredito.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
    o.SwaggerDoc("v1", new() { Title = "GesCrédito API", Version = "v1" }));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (origins.Length > 0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddHealthChecks().AddDbContextCheck<GesCreditoDbContext>("base-de-datos");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Necesario para WebApplicationFactory en las pruebas de integración.</summary>
public partial class Program;
