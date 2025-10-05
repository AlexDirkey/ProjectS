using Api.Services;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Tests;

public class Startup : IDisposable
{
    private readonly PostgreSqlContainer _pg;

    public Startup()
    {
        // Start en frisk Postgres-container til test
        _pg = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _pg.StartAsync().GetAwaiter().GetResult();
    }

    // Registrér alle services som i Program.cs – men mod test-databasen
    public void ConfigureServices(IServiceCollection services)
    {
        var conn = _pg.GetConnectionString(); // ssl ikke nødvendig lokalt i container
        services.AddDbContext<SpellsDbContext>(o => o.UseNpgsql(conn));

        services.AddScoped<ISpellService, SpellService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddHttpClient(); // til ImportService hvis du vil teste den senere
        services.AddScoped<IImportService, ImportService>();
    }

    // Kører efter ServiceProvider er bygget: klargør schema + tabeller
    public void Configure(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SpellsDbContext>();

        // Sørg for at schema 'spells' eksisterer og lad EF skabe tabellerne
        db.Database.ExecuteSqlRaw("create schema if not exists spells;");
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _pg.StopAsync().GetAwaiter().GetResult();
        _pg.Dispose();
    }
}