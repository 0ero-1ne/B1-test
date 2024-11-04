using System;
using System.Collections.Generic;

namespace Task2.Models;

public partial class Bill
{
    public long Id { get; set; }

    public int BookNumber { get; set; }

    public decimal InsaldoActive { get; set; }

    public decimal InsaldoPassive { get; set; }

    public decimal TurnoversDebit { get; set; }

    public decimal TurnoversCredit { get; set; }

    public decimal OutsaldoActive { get; set; }

    public decimal OutsaldoPassive { get; set; }

    public int BillType { get; set; }

    public int FileId { get; set; }

    public virtual BillType BillTypeNavigation { get; set; } = null!;

    public virtual File File { get; set; } = null!;
}
