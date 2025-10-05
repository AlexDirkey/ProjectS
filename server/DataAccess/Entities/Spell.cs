using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Spell
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Level { get; set; }

    public string? Schoolid { get; set; }

    public string? Castingtime { get; set; }

    public string? Range { get; set; }

    public string? Components { get; set; }

    public string? Duration { get; set; }

    public bool? Concentration { get; set; }

    public bool? Ritual { get; set; }

    public string? Description { get; set; }

    public string? Higherlevel { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual School? School { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
