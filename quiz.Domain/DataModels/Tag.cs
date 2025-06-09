using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Quiztag> Quiztags { get; set; } = new List<Quiztag>();
}
