using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class HocSinh
{
    public int IdHocSinh { get; set; }

    public string TenHocSinh { get; set; } = null!;

    public string? GioiTinh { get; set; }

    public DateTime? NgaySinh { get; set; }

    public int? IdLopHoc { get; set; }

    public int? IdTaiKhoan { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<BangDiem> BangDiems { get; set; } = new List<BangDiem>();

    public virtual LopHoc? IdLopHocNavigation { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
