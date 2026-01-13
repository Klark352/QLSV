using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class BangDiem
{
    public int IdBangDiem { get; set; }

    public double? Diem15phut { get; set; }

    public double? Diem45phut { get; set; }

    public double? DiemHocKi { get; set; }

    public int? IdMonHoc { get; set; }

    public int? IdHocSinh { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual HocSinh? IdHocSinhNavigation { get; set; }

    public virtual MonHoc? IdMonHocNavigation { get; set; }
}
