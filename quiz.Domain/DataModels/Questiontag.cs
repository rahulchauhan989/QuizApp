using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Questiontag
{
    public int Id { get; set; }

    public int Questionid { get; set; }

    public int Tagid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
