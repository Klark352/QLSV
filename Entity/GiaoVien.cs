using System;
using System.Collections.Generic;

namespace QLSV.Entity;

public partial class GiaoVien
{
    public int IdGiaoVien { get; set; }

    public string TenGiaoVien { get; set; } = null!;

    public string GioiTinh { get; set; } = null!;

    public DateTime NgaySinh { get; set; }

    public string? SoDienThoai { get; set; }

    public int? IdTaiKhoan { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
}
