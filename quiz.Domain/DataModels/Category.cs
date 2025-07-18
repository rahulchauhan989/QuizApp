﻿using System;
using System.Collections.Generic;

namespace quiz.Domain.DataModels;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public bool? Isdeleted { get; set; }

    public DateTime? Modifiedat { get; set; }

    public int? Createdby { get; set; }

    public int? Modifiedby { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<Userquizattempt> Userquizattempts { get; set; } = new List<Userquizattempt>();
}
