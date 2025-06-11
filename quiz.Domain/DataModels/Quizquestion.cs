using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Quizquestion
{
    public int Quizid { get; set; }

    public int Questionid { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
