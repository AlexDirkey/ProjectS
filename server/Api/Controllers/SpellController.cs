using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpellController : ControllerBase
{
    private readonly ISpellService _service;
    public SpellController(ISpellService service) => _service = service;

    // GET /api/Spell?q=&level=&classId=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<PagedResult<SpellResponseDto>>> Get(
        [FromQuery] string? q,
        [FromQuery] int? level,
        [FromQuery] string? classId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.SearchAsync(q, level, classId, page, pageSize, ct);
        return Ok(result);
    }

    // GET /api/Spell/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SpellResponseDto>> GetById(string id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // POST /api/Spell
    [HttpPost]
    public async Task<ActionResult<SpellResponseDto>> Create([FromBody] SpellCreateDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) when (ex.ParamName == "SchoolId" || ex.ParamName == "ClassIds")
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/Spell/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<SpellResponseDto>> Update(string id, [FromBody] SpellUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex) when (ex.ParamName == "SchoolId" || ex.ParamName == "ClassIds")
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/Spell/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
