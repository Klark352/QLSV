using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class LichNgayLopHoc
{
    public int IdLichNgay { get; set; }

    public int IdLopHoc { get; set; }

    public string NamHoc { get; set; } = null!;

    public int KiHoc { get; set; }

    public DateOnly Ngay { get; set; }

    public int Tuan { get; set; }

    public byte Thu { get; set; }

    public int TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;

    public virtual ICollection<ThoiKhoaBieuChiTiet> ThoiKhoaBieuChiTiets { get; set; } = new List<ThoiKhoaBieuChiTiet>();
}
