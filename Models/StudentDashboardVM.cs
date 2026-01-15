using System;
using System.Collections.Generic;

namespace QLSV.Models
{
    public class StudentDashboardVM
    {
        // Thông tin học sinh
        public int IdHocSinh { get; set; }
        public int? IdLopHoc { get; set; }
        public string TenHocSinh { get; set; } = "";
        public string? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }

        // Thông tin lớp học
        public string TenLopHoc { get; set; } = "";
        public string? TenKhoi { get; set; }
        public string? NamHoc { get; set; }
        public string? TenGiaoVienChuNhiem { get; set; }

        // Thống kê điểm cơ bản
        public int SoMonCoDiem { get; set; }
        public DateTime? CapNhatGanNhat { get; set; }
        public double? DiemTrungBinhTongHop { get; set; }

        // Danh sách điểm
        public List<SubjectScoreItem> DiemMoiNhatTheoMon { get; set; } = new();
        public List<RecentScoreItem> DiemCapNhatGanDay { get; set; } = new();

        // Thống kê số lượng điểm theo loại
        public int TongSoDiem15 { get; set; }
        public int TongSoDiem45 { get; set; }
        public int TongSoDiemHK { get; set; }

        // Thống kê điểm theo xếp loại
        public int DiemGioi { get; set; }
        public int DiemKha { get; set; }
        public int DiemTrungBinh { get; set; }
        public int DiemYeu { get; set; }

        // Top môn học
        public List<SubjectScoreItem> Top3MonDiemCao { get; set; } = new();
        public List<SubjectScoreItem> Top3MonCanCaiThien { get; set; } = new();

        // Thông tin lớp học
        public int TongHocSinhTrongLop { get; set; }
        public int SoMonHocTrongLop { get; set; }

        // Xếp hạng theo ĐTB trong lớp
        public int? XepHangTrongLop { get; set; }          // #mấy trong lớp
        public double? DtbTrongLop { get; set; }           // ĐTB dùng để xếp hạng 
        public double? TopPercentTrongLop { get; set; }    // top %

        public List<ClassRankRow> BangXepHangTop10 { get; set; } = new();
        public List<ClassRankRow> BangXepHangQuanhBan { get; set; } = new();


        public class ClassRankRow
        {
            public int IdHocSinh { get; set; }
            public string TenHocSinh { get; set; } = "";
            public double? Dtb { get; set; }
            public int Hang { get; set; }
        }

        public PagedResult<RecentScoreItem> RecentScoresPaged { get; set; } = new();
        public PagedResult<SubjectScoreItem> LatestBySubjectPaged { get; set; } = new();
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

        public DateTime NgayCapNhat { get; set; }
    }

    public class RecentScoreItem
    {
        public string TenMonHoc { get; set; } = "";
        public double? Diem15 { get; set; }
        public double? Diem45 { get; set; }
        public double? DiemHocKi { get; set; }
        public double? DiemTongHop { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class ScheduleItem
    {
        public int Tiet { get; set; }
        public string TenMonHoc { get; set; } = "";
        public string? TenGiaoVien { get; set; }
        public string? Phong { get; set; }
    }
}