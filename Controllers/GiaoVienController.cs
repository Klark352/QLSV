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
            return View();
        }

        public IActionResult NhapDiem()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult SuaDiem()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult BangDiem()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult DanhSachHocSinh()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult BaoCaoKetQua()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult BaoCaoHanhKiem()
        {
            SetHeaderInfoFromClaims();
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