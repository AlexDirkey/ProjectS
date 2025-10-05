using Api.DTOs;

namespace Api.Services;

public interface ISpellService
{
    Task<PagedResult<SpellResponseDto>> SearchAsync(
        string? q, int? level, string? classId, int page, int pageSize, CancellationToken ct);

    Task<SpellResponseDto?> GetByIdAsync(string id, CancellationToken ct);

    Task<SpellResponseDto> CreateAsync(SpellCreateDto dto, CancellationToken ct);

    Task<SpellResponseDto?> UpdateAsync(string id, SpellUpdateDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(string id, CancellationToken ct);
}