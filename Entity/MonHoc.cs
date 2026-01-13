using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class MonHoc
{
    public int IdMonHoc { get; set; }

    public string TenMonHoc { get; set; } = null!;

    public int? SoTietHoc { get; set; }

    public string? NamHoc { get; set; }

    public int KiHoc { get; set; }

    public int? IdGiaoVien { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public int SoTinChi { get; set; }

    public virtual ICollection<BangDiem> BangDiems { get; set; } = new List<BangDiem>();

    public virtual GiaoVien? IdGiaoVienNavigation { get; set; }

    public virtual ICollection<LopHocMonHoc> LopHocMonHocs { get; set; } = new List<LopHocMonHoc>();

    public virtual ICollection<ThoiKhoaBieuChiTiet> ThoiKhoaBieuChiTiets { get; set; } = new List<ThoiKhoaBieuChiTiet>();
}
