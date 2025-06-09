using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Useranswer
{
    public int Id { get; set; }

    public int Attemptid { get; set; }

    public int Questionid { get; set; }

    public int Optionid { get; set; }

    public bool? Iscorrect { get; set; }

    public virtual Userquizattempt Attempt { get; set; } = null!;

    public virtual Option Option { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
