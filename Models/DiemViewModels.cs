using System.Collections.Generic;

public class DiemHocKiViewModel
{
    public string NamHoc { get; set; }
    public int KiHoc { get; set; }
    public List<DiemMonRow> Rows { get; set; } = new();
    public double? DTB { get; set; }          // hệ 10
    public double? GPA4 { get; set; }         // hệ 4 (theo tín chỉ)  <-- THÊM
}

public class DiemMonRow
{
    public int IdMonHoc { get; set; }
    public string TenMonHoc { get; set; }
    public int? SoTietHoc { get; set; }
    public int? SoTinChi { get; set; }
    public double? Diem15p { get; set; }
    public double? Diem45p { get; set; }
    public double? DiemHK { get; set; }
    public double? DiemTongKetMon { get; set; }
    public double? DiemTongKetHe4 { get; set; }

}

