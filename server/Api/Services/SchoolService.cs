using Api.DTOs;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class SchoolService(SpellsDbContext db) : ISchoolService
{
    public async Task<List<SchoolResponse>> GetAllAsync(CancellationToken ct = default) =>
        await db.Schools.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SchoolResponse(s.Id, s.Name))
            .ToListAsync(ct);

    public async Task<SchoolResponse?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await db.Schools.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SchoolResponse(s.Id, s.Name))
            .FirstOrDefaultAsync(ct);

    public async Task<SchoolResponse> CreateAsync(SchoolCreate req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Id)) throw new ArgumentException("Id is required", nameof(req.Id));
        if (await db.Schools.AnyAsync(s => s.Id == req.Id, ct))
            throw new ArgumentException("Id already exists", nameof(req.Id));

        db.Schools.Add(new DataAccess.Entities.School { Id = req.Id, Name = req.Name });
        await db.SaveChangesAsync(ct);
        return new SchoolResponse(req.Id, req.Name);
    }

    public async Task<SchoolResponse?> UpdateAsync(string id, SchoolUpdate req, CancellationToken ct = default)
    {
        var entity = await db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return null;

        entity.Name = req.Name;
        await db.SaveChangesAsync(ct);
        return new SchoolResponse(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return false;

        db.Schools.Remove(entity);
        await db.SaveChangesAsync(ct);
        return true;
    }
}