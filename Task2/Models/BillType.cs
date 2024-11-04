using System;
using System.Collections.Generic;

namespace Task2.Models;

public partial class BillType
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
