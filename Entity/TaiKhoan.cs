using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class TaiKhoan
{
    public int IdTaiKhoan { get; set; }

    public string TenTaiKhoan { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? PhanQuyen { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public int TrangThai { get; set; }

    public virtual GiaoVien? GiaoVien { get; set; }

    public virtual HocSinh? HocSinh { get; set; }
}
