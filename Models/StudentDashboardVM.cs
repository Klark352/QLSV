using System;
using System.Collections.Generic;

namespace QLSV.Models
{
    public class StudentDashboardVM
    {
        public int IdHocSinh { get; set; }
        public int? IdLopHoc { get; set; }

        public string TenHocSinh { get; set; } = "";
        public string? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }

        public string TenLopHoc { get; set; } = "";
        public string? TenKhoi { get; set; }
        public string? NamHoc { get; set; }
        public string? TenGiaoVienChuNhiem { get; set; }

        public int SoMonCoDiem { get; set; }
        public DateTime? CapNhatGanNhat { get; set; }
        public double? DiemTrungBinhTongHop { get; set; }

        public List<SubjectScoreItem> DiemMoiNhatTheoMon { get; set; } = new();
        public List<RecentScoreItem> DiemCapNhatGanDay { get; set; } = new();
    }

    public class SubjectScoreItem
    {
        public int IdMonHoc { get; set; }
        public string TenMonHoc { get; set; } = "";
        public int KiHoc { get; set; }
        public string? NamHoc { get; set; }
        public string? TenGiaoVien { get; set; }

        public double? Diem15 { get; set; }
        public double? Diem45 { get; set; }
        public double? DiemHocKi { get; set; }
        public double? DiemTongHop { get; set; }

        public DateTime? NgayCapNhat { get; set; }
    }

    public class RecentScoreItem
    {
        public string TenMonHoc { get; set; } = "";
        public double? Diem15 { get; set; }
        public double? Diem45 { get; set; }
        public double? DiemHocKi { get; set; }
        public double? DiemTongHop { get; set; }
        public DateTime? NgayCapNhat { get; set; }
    }
}
