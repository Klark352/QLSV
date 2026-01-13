using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class LopHocMonHoc
{
    public int IdLopHoc { get; set; }

    public int IdMonHoc { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;

    public virtual MonHoc IdMonHocNavigation { get; set; } = null!;
}
