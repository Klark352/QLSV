using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class HanhKiem
{
    public int IdHanhKiem { get; set; }

    public int IdHocSinh { get; set; }

    public string NamHoc { get; set; } = null!;

    public int KiHoc { get; set; }

    public string XepLoai { get; set; } = null!;

    public string? NhanXet { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual HocSinh IdHocSinhNavigation { get; set; } = null!;
}
