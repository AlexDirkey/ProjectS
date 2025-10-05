using System;
using Api.Services;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Tests;

// Startup til test: spinner en Postgres-container op og bygger et DI-setup som i Program.cs
public class Startup : IDisposable
{
    // Holder reference til den kørende Testcontainers Postgres
    private readonly PostgreSqlContainer _pg;

    // Constructor: konfigurer og start containeren synkront (nemt i test-kontekst)
    public Startup()
    {
        _pg = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine") // lille og hurtig Postgres-image
            .WithDatabase("testdb")          // databasenavn i containeren
            .WithUsername("postgres")        // test-bruger
            .WithPassword("postgres")        // test-adgangskode
            .Build();

        // Start container (blokker til den er klar)
        _pg.StartAsync().GetAwaiter().GetResult();
    }

    // Registrér services som i Program.cs – men peg på test-databasen i containeren
    public void ConfigureServices(IServiceCollection services)
    {
        // Connection string fra containeren; ssl ikke nødvendigt lokalt i test
        var conn = _pg.GetConnectionString();

        // EF Core DbContext peger på test-containeren
        services.AddDbContext<SpellsDbContext>(o => o.UseNpgsql(conn));

        // Registrér app-services (samme som runtime)
        services.AddScoped<ISpellService, SpellService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<ISchoolService, SchoolService>();

        // HttpClient til import-service
        services.AddHttpClient();
        services.AddScoped<IImportService, ImportService>();
    }

    // Kør efter serviceprovider er bygget: sørg for schema og tabeller findes
    public void Configure(IServiceProvider provider)
    {
        // Scope, så vi kan resolve DbContext sikkert
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SpellsDbContext>();

        // Opret schema eksplicit (EnsureCreated laver ikke schema automatisk for alle providers)
        db.Database.ExecuteSqlRaw("create schema if not exists spells;");

        // Opret tabeller hvis de ikke findes (ingen migrations i rene test-kørsler)
        db.Database.EnsureCreated();
    }

    // Oprydning: stop og dispose containeren når testen er færdig
    public void Dispose()
    {
        _pg.StopAsync().GetAwaiter().GetResult();
        _pg.Dispose();
    }
}
