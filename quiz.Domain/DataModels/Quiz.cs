using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Totalmarks { get; set; }

    public int? Durationminutes { get; set; }

    public bool? Ispublic { get; set; }

    public int Categoryid { get; set; }

    public int Createdby { get; set; }

    public bool? Isdeleted { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Modifiedat { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User CreatedbyNavigation { get; set; } = null!;

    public virtual ICollection<Quiztag> Quiztags { get; set; } = new List<Quiztag>();

    public virtual ICollection<Userquizattempt> Userquizattempts { get; set; } = new List<Userquizattempt>();
}
