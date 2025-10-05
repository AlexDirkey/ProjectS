using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class School
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Spell> Spells { get; set; } = new List<Spell>();
}
