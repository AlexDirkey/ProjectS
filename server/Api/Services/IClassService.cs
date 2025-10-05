using Api.DTOs;

namespace Api.Services;

public interface IClassService
{
    Task<List<ClassResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ClassResponse?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ClassResponse> CreateAsync(ClassCreate req, CancellationToken ct = default);
    Task<ClassResponse?> UpdateAsync(string id, ClassUpdate req, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}