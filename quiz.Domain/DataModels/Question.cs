using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Question
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int? Marks { get; set; }

    public string? Difficulty { get; set; }

    public bool? Isdeleted { get; set; }

    public DateTime? Createdat { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? Modifierdat { get; set; }

    public int? Createdby { get; set; }

    public int? Updatedby { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual ICollection<Questiontag> Questiontags { get; set; } = new List<Questiontag>();

    public virtual ICollection<Useranswer> Useranswers { get; set; } = new List<Useranswer>();
}
