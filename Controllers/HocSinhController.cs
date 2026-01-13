using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV.Entity;
using QLSV.Models;
using System.Security.Claims;

namespace QLSV.Controllers
{
    [Authorize(Roles = "HocSinh")]
    public class HocSinhController : Controller
    {
        private readonly QlsvContext _db;
        public HocSinhController(QlsvContext db) => _db = db;

        // Công thức tổng hợp: 15'(1) - 45'(2) - HK(3)
        private static double? CalcTongHop(double? d15, double? d45, double? dhk)
        {
            double sum = 0; double w = 0;
            if (d15.HasValue) { sum += d15.Value * 1; w += 1; }
            if (d45.HasValue) { sum += d45.Value * 2; w += 2; }
            if (dhk.HasValue) { sum += dhk.Value * 3; w += 3; }
            if (w == 0) return null;
            return Math.Round(sum / w, 2);
        }

        public async Task<IActionResult> Dashboard()
        {
            // idTaiKhoan lấy từ Claims (đã set ở AuthController)
            var idTaiKhoanStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idTaiKhoanStr, out var idTaiKhoan))
                return RedirectToAction("Login", "Auth");

            // Lấy học sinh theo idTaiKhoan (schema mới)
            var hs = await _db.HocSinhs
                .Include(h => h.IdLopHocNavigation)
                    .ThenInclude(l => l!.IdGiaoVienChuNhiemNavigation)
                .FirstOrDefaultAsync(h => h.IdTaiKhoan == idTaiKhoan);

            if (hs == null)
                return RedirectToAction("AccessDenied", "Auth");

            var lop = hs.IdLopHocNavigation;
            var gvcn = lop?.IdGiaoVienChuNhiemNavigation;

            // Lấy tất cả điểm của HS + join MonHoc + GiaoVien
            var allScores = await _db.BangDiems
                .Where(b => b.IdHocSinh == hs.IdHocSinh)
                .Include(b => b.IdMonHocNavigation)
                    .ThenInclude(m => m!.IdGiaoVienNavigation)
                .ToListAsync();

            // Cập nhật gần nhất
            var capNhatGanNhat = allScores
                .OrderByDescending(x => x.NgayCapNhat)
                .Select(x => (DateTime?)x.NgayCapNhat)
                .FirstOrDefault();

            // Điểm cập nhật gần đây (top 8)
            var recent = allScores
                .OrderByDescending(x => x.NgayCapNhat)
                .Take(8)
                .Select(x => new RecentScoreItem
                {
                    TenMonHoc = x.IdMonHocNavigation?.TenMonHoc ?? "Môn học",
                    Diem15 = x.Diem15phut,
                    Diem45 = x.Diem45phut,
                    DiemHocKi = x.DiemHocKi,
                    DiemTongHop = CalcTongHop(x.Diem15phut, x.Diem45phut, x.DiemHocKi),
                    NgayCapNhat = x.NgayCapNhat
                })
                .ToList();

            // Điểm mới nhất theo từng môn (group by idMonHoc -> lấy bản ghi có ngayCapNhat mới nhất)
            var latestBySubject = allScores
                .Where(x => x.IdMonHocNavigation != null)
                .GroupBy(x => x.IdMonHoc)
                .Select(g => g.OrderByDescending(x => x.NgayCapNhat).First())
                .OrderBy(x => x.IdMonHocNavigation!.TenMonHoc)
                .Select(x => new SubjectScoreItem
                {
                    IdMonHoc = x.IdMonHoc ?? 0,
                    TenMonHoc = x.IdMonHocNavigation!.TenMonHoc,
                    KiHoc = x.IdMonHocNavigation!.KiHoc,
                    NamHoc = x.IdMonHocNavigation!.NamHoc,
                    TenGiaoVien = x.IdMonHocNavigation!.IdGiaoVienNavigation?.TenGiaoVien,

                    Diem15 = x.Diem15phut,
                    Diem45 = x.Diem45phut,
                    DiemHocKi = x.DiemHocKi,
                    DiemTongHop = CalcTongHop(x.Diem15phut, x.Diem45phut, x.DiemHocKi),

                    NgayCapNhat = x.NgayCapNhat
                })
                .ToList();

            // Điểm TB tổng hợp từ điểm tổng hợp mới nhất theo môn
            double? tb = null;
            var listTongHop = latestBySubject.Where(s => s.DiemTongHop.HasValue).Select(s => s.DiemTongHop!.Value).ToList();
            if (listTongHop.Count > 0) tb = Math.Round(listTongHop.Average(), 2);

            var vm = new StudentDashboardVM
            {
                IdHocSinh = hs.IdHocSinh,
                IdLopHoc = hs.IdLopHoc,

                TenHocSinh = hs.TenHocSinh ?? "",
                GioiTinh = hs.GioiTinh,
                NgaySinh = hs.NgaySinh,

                TenLopHoc = lop?.TenLopHoc ?? "",
                TenKhoi = lop?.TenKhoi,
                NamHoc = lop?.NamHoc,
                TenGiaoVienChuNhiem = gvcn?.TenGiaoVien,

                SoMonCoDiem = latestBySubject.Count,
                CapNhatGanNhat = capNhatGanNhat,
                DiemTrungBinhTongHop = tb,

                DiemMoiNhatTheoMon = latestBySubject,
                DiemCapNhatGanDay = recent
            };

            // Cho layout dùng (nếu bạn đang hiển thị tên)
            ViewBag.TenHocSinh = vm.TenHocSinh;
            ViewBag.TenLop = vm.TenLopHoc;

            return View(vm);
        }
    }
}
