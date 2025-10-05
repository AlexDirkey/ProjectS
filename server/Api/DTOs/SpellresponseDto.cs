// Api/DTOs/SpellResponseDto.cs
namespace Api.DTOs;

public class SpellResponseDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Level { get; set; }
    public string? SchoolId { get; set; }
    public string? CastingTime { get; set; }
    public string? Range { get; set; }
    public string? Components { get; set; }
    public string? Duration { get; set; }
    public bool Concentration { get; set; }
    public bool Ritual { get; set; }
    public string? Description { get; set; }
    public string? HigherLevel { get; set; }

    // ← Tilføjet: bruges i SpellService.MapToResponse og i projektioner
    public List<string> ClassNames { get; set; } = new();
}