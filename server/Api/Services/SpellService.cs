// Api/Services/SpellService.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.DTOs;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SpellService : ISpellService
    {
        private readonly SpellsDbContext _db;

        public SpellService(SpellsDbContext db)
        {
            _db = db;
        }

        // ---------------------- Helpers ----------------------

        private static SpellResponseDto MapToResponse(Spell s)
        {
            return new SpellResponseDto
            {
                Id            = s.Id,
                Name          = s.Name,
                Level         = s.Level,
                SchoolId      = s.Schoolid,
                CastingTime   = s.Castingtime,
                Range         = s.Range,
                Components    = s.Components,
                Duration      = s.Duration,
                Concentration = s.Concentration ?? false,
                Ritual        = s.Ritual ?? false,
                Description   = s.Description,
                HigherLevel   = s.Higherlevel,
                ClassNames    = s.Classes != null
                    ? s.Classes.Select(c => c.Name).OrderBy(n => n).ToList()
                    : new List<string>()
            };
        }

        private IQueryable<Spell> BuildQuery(string? q, int? level, string? classId)
        {
            var query = _db.Spells.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)));
            }

            if (level.HasValue)
                query = query.Where(s => s.Level == level.Value);

            if (!string.IsNullOrWhiteSpace(classId))
                query = query.Where(s => s.Classes.Any(c => c.Id == classId));

            return query.OrderBy(s => s.Level).ThenBy(s => s.Name);
        }

        // ---------------------- Queries ----------------------

        public async Task<PagedResult<SpellResponseDto>> SearchAsync(
            string? q, int? level, string? classId, int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 1000) pageSize = 1000;

            var baseQuery = BuildQuery(q, level, classId);

            var total = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SpellResponseDto
                {
                    Id            = s.Id,
                    Name          = s.Name,
                    Level         = s.Level,
                    SchoolId      = s.Schoolid,
                    CastingTime   = s.Castingtime,
                    Range         = s.Range,
                    Components    = s.Components,
                    Duration      = s.Duration,
                    Concentration = s.Concentration ?? false,
                    Ritual        = s.Ritual ?? false,
                    Description   = s.Description,
                    HigherLevel   = s.Higherlevel,
                    ClassNames    = s.Classes.Select(c => c.Name).OrderBy(n => n).ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<SpellResponseDto>(items, total, page, pageSize);
        }

        public async Task<SpellResponseDto?> GetByIdAsync(string id, CancellationToken ct)
        {
            var entity = await _db.Spells.AsNoTracking()
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return entity is null ? null : MapToResponse(entity);
        }

        // ---------------------- Commands ----------------------

        public async Task<SpellResponseDto> CreateAsync(SpellCreateDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required", nameof(dto.Name));

            // school: hvis angivet, skal den findes
            string? schoolId = null;
            if (!string.IsNullOrWhiteSpace(dto.SchoolId))
            {
                var schoolExists = await _db.Schools.AnyAsync(s => s.Id == dto.SchoolId, ct);
                if (!schoolExists)
                    throw new ArgumentException($"School '{dto.SchoolId}' does not exist", nameof(dto.SchoolId));
                schoolId = dto.SchoolId;
            }

            var id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id;

            var entity = new Spell
            {
                Id            = id,
                Name          = dto.Name.Trim(),
                Level         = dto.Level,
                Schoolid      = schoolId,
                Castingtime   = dto.CastingTime,
                Range         = dto.Range,
                Components    = dto.Components,
                Duration      = dto.Duration,
                Concentration = dto.Concentration,
                Ritual        = dto.Ritual,
                Description   = dto.Description,
                Higherlevel   = dto.HigherLevel
            };

            _db.Spells.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Many-to-many: tilføj links via navigation
            if (dto.ClassIds is { Count: > 0 })
            {
                var classes = await _db.Classes
                    .Where(c => dto.ClassIds.Contains(c.Id))
                    .ToListAsync(ct);

                foreach (var c in classes)
                    entity.Classes.Add(c);

                await _db.SaveChangesAsync(ct);
            }

            // returnér med ClassNames (projektion for at sikre navigation)
            var withClasses = await _db.Spells
                .AsNoTracking()
                .Where(s => s.Id == entity.Id)
                .Select(s => new SpellResponseDto
                {
                    Id            = s.Id,
                    Name          = s.Name,
                    Level         = s.Level,
                    SchoolId      = s.Schoolid,
                    CastingTime   = s.Castingtime,
                    Range         = s.Range,
                    Components    = s.Components,
                    Duration      = s.Duration,
                    Concentration = s.Concentration ?? false,
                    Ritual        = s.Ritual ?? false,
                    Description   = s.Description,
                    HigherLevel   = s.Higherlevel,
                    ClassNames    = s.Classes.Select(c => c.Name).OrderBy(n => n).ToList()
                })
                .FirstAsync(ct);

            return withClasses;
        }

        public async Task<SpellResponseDto?> UpdateAsync(string id, SpellUpdateDto dto, CancellationToken ct)
        {
            var entity = await _db.Spells
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (entity is null) return null;

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required", nameof(dto.Name));

            // School validate hvis angivet
            if (!string.IsNullOrWhiteSpace(dto.SchoolId))
            {
                var exists = await _db.Schools.AnyAsync(s => s.Id == dto.SchoolId, ct);
                if (!exists)
                    throw new ArgumentException($"School '{dto.SchoolId}' does not exist", nameof(dto.SchoolId));
            }

            entity.Name          = dto.Name.Trim();
            entity.Level         = dto.Level;
            entity.Schoolid      = dto.SchoolId;
            entity.Castingtime   = dto.CastingTime;
            entity.Range         = dto.Range;
            entity.Components    = dto.Components;
            entity.Duration      = dto.Duration;
            entity.Concentration = dto.Concentration;
            entity.Ritual        = dto.Ritual;
            entity.Description   = dto.Description;
            entity.Higherlevel   = dto.HigherLevel;

            // Opdatér klasser hvis angivet
            if (dto.ClassIds is not null)
            {
                entity.Classes.Clear();
                if (dto.ClassIds.Count > 0)
                {
                    var classes = await _db.Classes
                        .Where(c => dto.ClassIds.Contains(c.Id))
                        .ToListAsync(ct);

                    foreach (var c in classes)
                        entity.Classes.Add(c);
                }
            }

            await _db.SaveChangesAsync(ct);

            // returnér projiceret svar inkl. klassenavne
            var result = await _db.Spells
                .AsNoTracking()
                .Where(s => s.Id == entity.Id)
                .Select(s => new SpellResponseDto
                {
                    Id            = s.Id,
                    Name          = s.Name,
                    Level         = s.Level,
                    SchoolId      = s.Schoolid,
                    CastingTime   = s.Castingtime,
                    Range         = s.Range,
                    Components    = s.Components,
                    Duration      = s.Duration,
                    Concentration = s.Concentration ?? false,
                    Ritual        = s.Ritual ?? false,
                    Description   = s.Description,
                    HigherLevel   = s.Higherlevel,
                    ClassNames    = s.Classes.Select(c => c.Name).OrderBy(n => n).ToList()
                })
                .FirstAsync(ct);

            return result;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct)
        {
            var entity = await _db.Spells.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity is null) return false;

            _db.Spells.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}



