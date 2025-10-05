using Api.DTOs;
using Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests;

// Tests af ClassService via DI (som i app'en)
public class ClassServiceTests
{
    // Service vi tester
    private readonly IClassService _svc;

    // xUnit injicerer ServiceProvider fra Startup (test)
    public ClassServiceTests(IServiceProvider sp)
    {
        // Hent den konkrete implementation fra DI
        _svc = sp.GetRequiredService<IClassService>();
    }

    [Fact]
    public async Task Create_Then_List_Works()
    {
        // Arrange: unikt id til testen
        var id = "wizard_test";

        // Act: opret og hent liste
        await _svc.CreateAsync(new ClassCreate(id, "Wizard Test"));
        var all = await _svc.GetAllAsync();

        // Assert: den nye class findes i listen
        Assert.Contains(all, c => c.Id == id);
    }

    [Fact]
    public async Task Update_Then_Delete_Works()
    {
        // Arrange: opret først en class
        var id = "cleric_test";
        await _svc.CreateAsync(new ClassCreate(id, "Cleric A"));

        // Act: opdatér navn og slet bagefter
        var updated = await _svc.UpdateAsync(id, new ClassUpdate("Cleric B"));
        var ok = await _svc.DeleteAsync(id);

        // Assert: navn er opdateret og delete returnerer true
        Assert.Equal("Cleric B", updated!.Name);
        Assert.True(ok);
    }
}