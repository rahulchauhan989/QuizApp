using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Quiztag
{
    public int Id { get; set; }

    public int Quizid { get; set; }

    public int Tagid { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
