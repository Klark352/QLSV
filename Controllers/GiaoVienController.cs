using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV.Entity;
using QLSV.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace QLSV.Controllers
{
    [Authorize(Roles = "GiaoVien")]
    public class GiaoVienController : Controller
    {
        private readonly QlsvContext _db;

        public GiaoVienController(QlsvContext db)
        {
            _db = db;
        }

        private int? GetTeacherId()
        {
            var idStr = User.FindFirstValue("IdGiaoVien") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : null;
        }

        private void SetHeaderInfoFromClaims()
        {
            ViewBag.TenGiaoVien = User.FindFirstValue("TenGiaoVien") ?? User.Identity?.Name ?? "Giáo viên";
            ViewBag.MonHoc = User.FindFirstValue("MonHoc") ?? "";
        }

        public async Task<IActionResult> Dashboard()
        {
            SetHeaderInfoFromClaims();
            var vm = await BuildOverviewVm();
            if (vm == null) return RedirectToAction("Login", "Auth");
            return View(vm);
        }

        public async Task<IActionResult> LopChuNhiem()
        {
            SetHeaderInfoFromClaims();
            var vm = await BuildOverviewVm();
            if (vm == null) return RedirectToAction("Login", "Auth");
            return View(vm);
        }

        public async Task<IActionResult> DanhSachLop()
        {
            SetHeaderInfoFromClaims();
            var vm = await BuildOverviewVm();
            if (vm == null) return RedirectToAction("Login", "Auth");
            return View(vm);
        }

        public IActionResult LichGiangDay()
        {
            SetHeaderInfoFromClaims();
            return RedirectToAction(nameof(LichGiangDayAsync));
        }

        public async Task<IActionResult> LichGiangDayAsync()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var items = await _db.ThoiKhoaBieuChiTiets
                .AsNoTracking()
                .Include(x => x.IdLichNgayNavigation)
                    .ThenInclude(ln => ln.IdLopHocNavigation)
                .Include(x => x.IdMonHocNavigation)
                .Where(x => x.IdGiaoVien == id.Value)
                .OrderBy(x => x.IdLichNgayNavigation!.Tuan)
                .ThenBy(x => x.IdLichNgayNavigation!.Thu)
                .ThenBy(x => x.Tiet)
                .ToListAsync();

            return View("LichGiangDay", items);
        }

        public async Task<IActionResult> NhapDiem()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var mon = await _db.MonHocs.AsNoTracking()
                .Where(m => m.IdGiaoVien == id.Value)
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            return View(mon);
        }

        public async Task<IActionResult> SuaDiem()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var mon = await _db.MonHocs.AsNoTracking()
                .Where(m => m.IdGiaoVien == id.Value)
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            return View(mon);
        }

        public async Task<IActionResult> BangDiem()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var monIds = await _db.MonHocs.AsNoTracking()
                .Where(m => m.IdGiaoVien == id.Value)
                .Select(m => m.IdMonHoc)
                .ToListAsync();

            var scores = await _db.BangDiems.AsNoTracking()
                .Include(b => b.IdHocSinhNavigation)
                .Include(b => b.IdMonHocNavigation)
                .Where(b => monIds.Contains(b.IdMonHoc ?? 0))
                .OrderByDescending(b => b.NgayCapNhat)
                .Take(100)
                .ToListAsync();

            return View(scores);
        }

        public async Task<IActionResult> DanhSachHocSinh()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var lopIds = await _db.LopHocMonHocs.AsNoTracking()
                .Include(x => x.IdMonHocNavigation)
                .Where(x => x.IdMonHocNavigation!.IdGiaoVien == id.Value)
                .Select(x => x.IdLopHoc)
                .ToListAsync();

            var lopCn = await _db.LopHocs.AsNoTracking()
                .Where(l => l.IdGiaoVienChuNhiem == id.Value)
                .Select(l => l.IdLopHoc)
                .ToListAsync();

            lopIds.AddRange(lopCn);
            lopIds = lopIds.Distinct().ToList();

            var hs = await _db.HocSinhs.AsNoTracking()
                .Include(h => h.IdLopHocNavigation)
                .Where(h => h.IdLopHoc != null && lopIds.Contains(h.IdLopHoc.Value))
                .OrderBy(h => h.TenHocSinh)
                .ToListAsync();

            return View(hs);
        }

        public async Task<IActionResult> BaoCaoKetQua()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var monIds = await _db.MonHocs.AsNoTracking()
                .Where(m => m.IdGiaoVien == id.Value)
                .Select(m => m.IdMonHoc)
                .ToListAsync();

            ViewBag.TongBangDiem = await _db.BangDiems.CountAsync(b => monIds.Contains(b.IdMonHoc ?? 0));
            ViewBag.CapNhatGanNhat = await _db.BangDiems
                .Where(b => monIds.Contains(b.IdMonHoc ?? 0))
                .MaxAsync(b => (DateTime?)b.NgayCapNhat);

            return View();
        }

        public async Task<IActionResult> BaoCaoHanhKiem()
        {
            SetHeaderInfoFromClaims();
            var id = GetTeacherId();
            if (id == null) return RedirectToAction("Login", "Auth");

            var lopCn = await _db.LopHocs.AsNoTracking()
                .Where(l => l.IdGiaoVienChuNhiem == id.Value)
                .Select(l => l.IdLopHoc)
                .ToListAsync();

            ViewBag.TongHocSinhChuNhiem = await _db.HocSinhs.CountAsync(h => h.IdLopHoc != null && lopCn.Contains(h.IdLopHoc.Value));
            return View();
        }

        private async Task<GiaoVienOverviewVM?> BuildOverviewVm()
        {
            var id = GetTeacherId();
            if (id == null) return null;

            var gv = await _db.GiaoViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdGiaoVien == id.Value);
            if (gv == null) return null;

            var lopChuNhiem = await _db.LopHocs.AsNoTracking()
                .Include(l => l.HocSinhs)
                .Where(l => l.IdGiaoVienChuNhiem == id.Value)
                .ToListAsync();

            var monPhuTrach = await _db.MonHocs.AsNoTracking()
                .Where(m => m.IdGiaoVien == id.Value)
                .ToListAsync();

            var lopGiangDay = await _db.LopHocMonHocs.AsNoTracking()
                .Include(x => x.IdLopHocNavigation)
                .Include(x => x.IdMonHocNavigation)
                .Where(x => x.IdMonHocNavigation!.IdGiaoVien == id.Value)
                .ToListAsync();

            var lopGiangDayDistinct = lopGiangDay
                .Select(x => x.IdLopHocNavigation)
                .Where(l => l != null)
                .DistinctBy(l => l!.IdLopHoc)
                .ToList();

            var lopIds = lopGiangDayDistinct.Select(l => l!.IdLopHoc).ToList();
            if (lopChuNhiem.Any())
            {
                lopIds.AddRange(lopChuNhiem.Select(l => l.IdLopHoc));
            }

            lopIds = lopIds.Distinct().ToList();
            var soHocSinh = await _db.HocSinhs.AsNoTracking().CountAsync(h => h.IdLopHoc != null && lopIds.Contains(h.IdLopHoc.Value));

            var vm = new GiaoVienOverviewVM
            {
                TenGiaoVien = gv.TenGiaoVien,
                GioiTinh = gv.GioiTinh,
                SoLopChuNhiem = lopChuNhiem.Count,
                SoMonPhuTrach = monPhuTrach.Count,
                SoLopGiangDay = lopGiangDayDistinct.Count,
                SoHocSinh = soHocSinh,
                LopChuNhiem = lopChuNhiem.Select(l => new HomeroomInfo
                {
                    IdLopHoc = l.IdLopHoc,
                    TenLopHoc = l.TenLopHoc,
                    TenKhoi = l.TenKhoi,
                    NamHoc = l.NamHoc,
                    HocSinhs = l.HocSinhs.OrderBy(h => h.TenHocSinh).ToList()
                }).ToList(),
                LopGiangDay = lopGiangDayDistinct.Select(l => new LopGiangDayItem
                {
                    IdLopHoc = l!.IdLopHoc,
                    TenLopHoc = l.TenLopHoc,
                    TenKhoi = l.TenKhoi,
                    NamHoc = l.NamHoc,
                    SoMonPhuTrach = lopGiangDay.Count(x => x.IdLopHoc == l.IdLopHoc)
                }).OrderBy(x => x.TenLopHoc).ToList()
            };

            return vm;
        }
    }
}