using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController(IImportService svc) : ControllerBase
{
    [HttpPost("dnd5e")]
    public async Task<IActionResult> Import(CancellationToken ct)
    {
        var count = await svc.ImportSpellsFromDnd5eAsync(ct);
        return Ok(new { imported = count });
    }
}