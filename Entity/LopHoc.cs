using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class LopHoc
{
    public int IdLopHoc { get; set; }

    public string TenLopHoc { get; set; } = null!;

    public string? TenKhoi { get; set; }

    public string? NamHoc { get; set; }

    public int? IdGiaoVienChuNhiem { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<HocSinh> HocSinhs { get; set; } = new List<HocSinh>();

    public virtual GiaoVien? IdGiaoVienChuNhiemNavigation { get; set; }

    public virtual ICollection<LopHocMonHoc> LopHocMonHocs { get; set; } = new List<LopHocMonHoc>();
}
