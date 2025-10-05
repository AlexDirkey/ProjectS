using System.Net.Http.Json;
using Api.Integrations;       // ← dine DnD5e DTO’er i Dnd5eImports.cs
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;         // ← VIGTIGT: samme namespace som i Program.cs-using

public interface IImportService
{
    Task<int> ImportSpellsFromDnd5eAsync(CancellationToken ct = default);
}

public sealed class ImportService(HttpClient http, SpellsDbContext db) : IImportService
{
    private const string Base = "https://www.dnd5eapi.co";

    public async Task<int> ImportSpellsFromDnd5eAsync(CancellationToken ct = default)
    {
        var list = await http.GetFromJsonAsync<Dnd5eSpellListDto>($"{Base}/api/spells", ct)
                   ?? new Dnd5eSpellListDto();

        var totalImported = 0;

        foreach (var item in list.Results)
        {
            var detail = await http.GetFromJsonAsync<Dnd5eSpellDetailDto>($"{Base}{item.Url}", ct);
            if (detail is null) continue;

            // upsert school
            string? schoolId = null;
            if (!string.IsNullOrWhiteSpace(detail.School?.Index))
            {
                schoolId = detail.School.Index;
                if (await db.Schools.FindAsync([schoolId], ct) is null)
                    db.Schools.Add(new School { Id = schoolId, Name = detail.School.Name });
            }

            // upsert classes
            var classIds = new List<string>();
            foreach (var c in detail.Classes)
            {
                classIds.Add(c.Index);
                if (await db.Classes.FindAsync([c.Index], ct) is null)
                    db.Classes.Add(new Class { Id = c.Index, Name = c.Name });
            }

            // upsert spell
            var id = detail.Index;
            var spell = await db.Spells
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            var components = detail.Components is { Count: >0 } ? string.Join(",", detail.Components) : null;
            var desc       = detail.Desc is { Count: >0 } ? string.Join("\n", detail.Desc) : null;
            var higher     = detail.Higher_Level is { Count: >0 } ? string.Join("\n", detail.Higher_Level) : null;

            if (spell is null)
            {
                spell = new Spell
                {
                    Id = id,
                    Name = detail.Name,
                    Level = detail.Level,
                    Schoolid = schoolId,
                    Castingtime = detail.Casting_Time,
                    Range = detail.Range,
                    Components = components,
                    Duration = detail.Duration,
                    Concentration = detail.Concentration ?? false,
                    Ritual = detail.Ritual ?? false,
                    Description = desc,
                    Higherlevel = higher,
                    Createdat = DateTime.UtcNow
                };

                if (classIds.Count > 0)
                {
                    var cls = await db.Classes.Where(x => classIds.Contains(x.Id)).ToListAsync(ct);
                    foreach (var c in cls) spell.Classes.Add(c);
                }

                db.Spells.Add(spell);
                totalImported++;
            }
            else
            {
                spell.Name = detail.Name;
                spell.Level = detail.Level;
                spell.Schoolid = schoolId;
                spell.Castingtime = detail.Casting_Time;
                spell.Range = detail.Range;
                spell.Components = components;
                spell.Duration = detail.Duration;
                spell.Concentration = detail.Concentration ?? false;
                spell.Ritual = detail.Ritual ?? false;
                spell.Description = desc;
                spell.Higherlevel = higher;

                spell.Classes.Clear();
                if (classIds.Count > 0)
                {
                    var cls = await db.Classes.Where(x => classIds.Contains(x.Id)).ToListAsync(ct);
                    foreach (var c in cls) spell.Classes.Add(c);
                }
            }

            if (totalImported % 25 == 0) await db.SaveChangesAsync(ct);
        }

        await db.SaveChangesAsync(ct);
        return totalImported;
    }
}
