using Api.DTOs;

namespace Api.Services;

public interface ISchoolService
{
    Task<List<SchoolResponse>> GetAllAsync(CancellationToken ct = default);
    Task<SchoolResponse?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<SchoolResponse> CreateAsync(SchoolCreate req, CancellationToken ct = default);
    Task<SchoolResponse?> UpdateAsync(string id, SchoolUpdate req, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}