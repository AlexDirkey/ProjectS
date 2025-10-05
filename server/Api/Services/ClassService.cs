using Api.DTOs;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ClassService(SpellsDbContext db) : IClassService
{
    public async Task<List<ClassResponse>> GetAllAsync(CancellationToken ct = default) =>
        await db.Classes.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ClassResponse(c.Id, c.Name))
            .ToListAsync(ct);

    public async Task<ClassResponse?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await db.Classes.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClassResponse(c.Id, c.Name))
            .FirstOrDefaultAsync(ct);

    public async Task<ClassResponse> CreateAsync(ClassCreate req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Id)) throw new ArgumentException("Id is required", nameof(req.Id));
        if (await db.Classes.AnyAsync(c => c.Id == req.Id, ct)) throw new ArgumentException("Id already exists", nameof(req.Id));

        db.Classes.Add(new DataAccess.Entities.Class { Id = req.Id, Name = req.Name });
        await db.SaveChangesAsync(ct);
        return new ClassResponse(req.Id, req.Name);
    }

    public async Task<ClassResponse?> UpdateAsync(string id, ClassUpdate req, CancellationToken ct = default)
    {
        var entity = await db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return null;

        entity.Name = req.Name;
        await db.SaveChangesAsync(ct);
        return new ClassResponse(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await db.Classes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return false;

        db.Classes.Remove(entity);
        await db.SaveChangesAsync(ct);
        return true;
    }
}