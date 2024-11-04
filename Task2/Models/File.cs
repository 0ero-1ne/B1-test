using System;
using System.Collections.Generic;

namespace Task2.Models;

public partial class File
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int BankId { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
