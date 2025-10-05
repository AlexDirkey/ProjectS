using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolController(ISchoolService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SchoolResponse>>> Get(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolResponse>> GetById(string id, CancellationToken ct) =>
        (await service.GetByIdAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SchoolResponse>> Create([FromBody] SchoolCreate req, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SchoolResponse>> Update(string id, [FromBody] SchoolUpdate req, CancellationToken ct) =>
        (await service.UpdateAsync(id, req, ct)) is { } updated ? Ok(updated) : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}