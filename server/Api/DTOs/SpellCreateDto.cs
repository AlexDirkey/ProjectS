using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class SpellCreateDto
{
    public string? Id { get; set; }

    [Required, MinLength(2)]
    public string Name { get; set; } = default!;

    [Range(0, 9)]
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

    // NY: many-to-many til classes (valgfri)
    public List<string> ClassIds { get; set; } = new();
}