using Api.DTOs;
using Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests;

// Simple integrationstests for ClassService med in-memory DI fra test-Startup
public class ClassServiceTests
{
    // System under test (SUT)
    private readonly IClassService _svc;

    // Test-constructør: får ServiceProvider injiceret af xUnit.DependencyInjection
    public ClassServiceTests(IServiceProvider sp)
    {
        // Resolve ClassService fra DI, så vi tester den rigtige wiring og DbContext
        _svc = sp.GetRequiredService<IClassService>();
    }

    [Fact]
    public async Task Create_Then_List_Works()
    {
        // Arrange: unikt id for klassen i testen
        var id = "wizard_test";

        // Act: opret en class og hent alle
        await _svc.CreateAsync(new ClassCreate(id, "Wizard Test"));
        var all = await _svc.GetAllAsync();

        // Assert: den nye class skal være i listen
        Assert.Contains(all, c => c.Id == id);
    }

    [Fact]
    public async Task Update_Then_Delete_Works()
    {
        // Arrange: opret en class der kan opdateres/slettes
        var id = "cleric_test";
        await _svc.CreateAsync(new ClassCreate(id, "Cleric A"));

        // Act: opdatér navnet og slet bagefter
        var updated = await _svc.UpdateAsync(id, new ClassUpdate("Cleric B"));
        var ok = await _svc.DeleteAsync(id);

        // Assert: navn blev ændret, og delete returnerer true
        Assert.Equal("Cleric B", updated!.Name);
        Assert.True(ok);
    }
}