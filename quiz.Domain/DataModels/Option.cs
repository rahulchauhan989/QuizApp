using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Option
{
    public int Id { get; set; }

    public int Questionid { get; set; }

    public string Text { get; set; } = null!;

    public bool Iscorrect { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<Useranswer> Useranswers { get; set; } = new List<Useranswer>();
}
