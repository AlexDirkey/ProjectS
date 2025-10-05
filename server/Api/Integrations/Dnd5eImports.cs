// server/Api/Integrations/Dnd5eImports.cs
// DTO’er til dnd5eapi.co – samlet i én fil og et enkelt namespace, så du slipper for flere mapper.

namespace Api.Integrations;

// /api/spells (liste)
public sealed class Dnd5eSpellListDto
{
    public int Count { get; set; }
    public List<Dnd5eListItem> Results { get; set; } = new();
}

public sealed class Dnd5eListItem
{
    public string Index { get; set; } = default!;
    public string Name  { get; set; } = default!;
    public string Url   { get; set; } = default!;
}

// /api/spells/{index} (detalje)
public sealed class Dnd5eSpellDetailDto
{
    public string Index { get; set; } = default!;
    public string Name  { get; set; } = default!;
    public int Level    { get; set; }

    public Dnd5eSchool School { get; set; } = default!;
    public List<Dnd5eNamedIndex> Classes { get; set; } = new();

    // bemærk: feltnavne matcher JSON (underscore/camel fra API’et)
    public string? Casting_Time { get; set; }  // "1 action"
    public string? Range        { get; set; }
    public List<string>? Components  { get; set; }
    public string? Duration     { get; set; }
    public bool?   Concentration{ get; set; }
    public bool?   Ritual       { get; set; }
    public List<string>? Desc         { get; set; }
    public List<string>? Higher_Level { get; set; }
}

public sealed class Dnd5eSchool
{
    public string Index { get; set; } = default!;
    public string Name  { get; set; } = default!;
}

public sealed class Dnd5eNamedIndex
{
    public string Index { get; set; } = default!;
    public string Name  { get; set; } = default!;
}