using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Userquizattempt
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Quizid { get; set; }

    public int? Score { get; set; }

    public int? Timespent { get; set; }

    public DateTime? Startedat { get; set; }

    public DateTime? Endedat { get; set; }

    public bool? Issubmitted { get; set; }

    public int? Categoryid { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Useranswer> Useranswers { get; set; } = new List<Useranswer>();
}
