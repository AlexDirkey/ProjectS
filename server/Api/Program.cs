using Api.Services;
using DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Kør altid på en kendt lokal HTTP-port (skift evt. 5080 hvis optaget)
builder.WebHost.UseUrls("http://localhost:5080");

// Connection string: brug CONN_STR env var hvis sat, ellers appsettings.json
var connStr =
    Environment.GetEnvironmentVariable("CONN_STR")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string. Set CONN_STR env var or add it to appsettings.json.");

builder.Services.AddDbContext<SpellsDbContext>(opt => opt.UseNpgsql(connStr));

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (åben – kan strammes senere)
builder.Services.AddCors();

// DI for services
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<ISpellService, SpellService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IImportService, ImportService>();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Redirect root til Swagger, så der altid er noget at se på /
app.MapGet("/", () => Results.Redirect("/swagger"));

// CORS
app.UseCors(cfg => cfg
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
    .SetIsOriginAllowed(_ => true));

// Controllers
app.MapControllers();

app.Run();

