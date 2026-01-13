using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class ThoiKhoaBieuChiTiet
{
    public int IdTkbCt { get; set; }

    public int IdLichNgay { get; set; }

    public byte Tiet { get; set; }

    public int? IdMonHoc { get; set; }

    public int? IdGiaoVien { get; set; }

    public string? Phong { get; set; }

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual GiaoVien? IdGiaoVienNavigation { get; set; }

    public virtual LichNgayLopHoc IdLichNgayNavigation { get; set; } = null!;

    public virtual MonHoc? IdMonHocNavigation { get; set; }
}
