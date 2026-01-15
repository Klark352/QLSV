using System.Collections.Generic;
using QLSV.Entity;

namespace QLSV.Models
{
    public class GiaoVienOverviewVM
    {
        public string TenGiaoVien { get; set; } = "";
        public string GioiTinh { get; set; } = "";

        public int SoLopChuNhiem { get; set; }
        public int SoMonPhuTrach { get; set; }
        public int SoLopGiangDay { get; set; }
        public int SoHocSinh { get; set; }

        public List<HomeroomInfo> LopChuNhiem { get; set; } = new();
        public List<LopGiangDayItem> LopGiangDay { get; set; } = new();
    }

    public class HomeroomInfo
    {
        public int? IdLopHoc { get; set; }
        public string? TenLopHoc { get; set; }
        public string? TenKhoi { get; set; }
        public string? NamHoc { get; set; }
        public List<HocSinh> HocSinhs { get; set; } = new();
    }

    public class LopGiangDayItem
    {
        public int IdLopHoc { get; set; }
        public string TenLopHoc { get; set; } = "";
        public string? TenKhoi { get; set; }
        public string? NamHoc { get; set; }
        public int SoMonPhuTrach { get; set; }
    }
}
