using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV.Entity;
using QLSV.Models;

namespace QLSV.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly QlsvContext _db;

        public AdminController(QlsvContext db)
        {
            _db = db;
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard() => View();

        // === Học sinh ===
        public async Task<IActionResult> HocSinh(string? q = null, int? idLop = null, int page = 1, int pageSize = 20)
        {
            var query = _db.HocSinhs
                .AsNoTracking()
                .Include(x => x.IdLopHocNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim();
                query = query.Where(x => x.TenHocSinh.Contains(key));
            }

            if (idLop.HasValue)
            {
                query = query.Where(x => x.IdLopHoc == idLop.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.TenHocSinh)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.IdLop = idLop;

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateHocSinh()
        {
            return View(new StudentVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHocSinh(StudentVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = new HocSinh
            {
                TenHocSinh = model.TenHocSinh!.Trim(),
                GioiTinh = model.GioiTinh,
                NgaySinh = model.NgaySinh,
                IdLopHoc = model.IdLopHoc,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            _db.HocSinhs.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(HocSinh));
        }

        [HttpGet]
        public async Task<IActionResult> EditHocSinh(int id)
        {
            var hs = await _db.HocSinhs.AsNoTracking().FirstOrDefaultAsync(x => x.IdHocSinh == id);
            if (hs == null) return NotFound();

            var vm = new StudentVM
            {
                Id = hs.IdHocSinh,
                TenHocSinh = hs.TenHocSinh,
                GioiTinh = hs.GioiTinh,
                NgaySinh = hs.NgaySinh,
                IdLopHoc = hs.IdLopHoc
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHocSinh(int id, StudentVM model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var hs = await _db.HocSinhs.FirstOrDefaultAsync(x => x.IdHocSinh == id);
            if (hs == null) return NotFound();

            hs.TenHocSinh = model.TenHocSinh!.Trim();
            hs.GioiTinh = model.GioiTinh;
            hs.NgaySinh = model.NgaySinh;
            hs.IdLopHoc = model.IdLopHoc;
            hs.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(HocSinh));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHocSinh(int id)
        {
            var hs = await _db.HocSinhs
                .Include(x => x.BangDiems)
                .Include(x => x.HanhKiems)
                .FirstOrDefaultAsync(x => x.IdHocSinh == id);
            if (hs == null) return NotFound();

            if (hs.BangDiems.Any() || hs.HanhKiems.Any())
            {
                TempData["Error"] = "Không thể xóa học sinh có điểm hoặc hạnh kiểm.";
                return RedirectToAction(nameof(HocSinh));
            }

            _db.HocSinhs.Remove(hs);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(HocSinh));
        }

        // === Giáo viên ===
        public async Task<IActionResult> GiaoVien(string? q = null, int page = 1, int pageSize = 20)
        {
            var query = _db.GiaoViens.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim();
                query = query.Where(x => x.TenGiaoVien.Contains(key));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.TenGiaoVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateGiaoVien()
        {
            return View(new TeacherVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGiaoVien(TeacherVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var gv = new GiaoVien
            {
                TenGiaoVien = model.TenGiaoVien!.Trim(),
                GioiTinh = model.GioiTinh,
                NgaySinh = model.NgaySinh,
                SoDienThoai = model.SoDienThoai,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            _db.GiaoViens.Add(gv);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GiaoVien));
        }

        [HttpGet]
        public async Task<IActionResult> EditGiaoVien(int id)
        {
            var gv = await _db.GiaoViens.AsNoTracking().FirstOrDefaultAsync(x => x.IdGiaoVien == id);
            if (gv == null) return NotFound();

            var vm = new TeacherVM
            {
                Id = gv.IdGiaoVien,
                TenGiaoVien = gv.TenGiaoVien,
                GioiTinh = gv.GioiTinh,
                NgaySinh = gv.NgaySinh,
                SoDienThoai = gv.SoDienThoai
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGiaoVien(int id, TeacherVM model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var gv = await _db.GiaoViens.FirstOrDefaultAsync(x => x.IdGiaoVien == id);
            if (gv == null) return NotFound();

            gv.TenGiaoVien = model.TenGiaoVien!.Trim();
            gv.GioiTinh = model.GioiTinh;
            gv.NgaySinh = model.NgaySinh;
            gv.SoDienThoai = model.SoDienThoai;
            gv.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GiaoVien));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGiaoVien(int id)
        {
            var gv = await _db.GiaoViens
                .Include(x => x.LopHocs)
                .Include(x => x.MonHocs)
                .FirstOrDefaultAsync(x => x.IdGiaoVien == id);
            if (gv == null) return NotFound();

            if (gv.LopHocs.Any() || gv.MonHocs.Any())
            {
                TempData["Error"] = "Không thể xóa giáo viên đang phụ trách lớp hoặc môn.";
                return RedirectToAction(nameof(GiaoVien));
            }

            _db.GiaoViens.Remove(gv);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GiaoVien));
        }

        public IActionResult LopHoc() => View();
        public IActionResult MonHoc() => View();
        public IActionResult BangDiem() => View();
        public IActionResult TaiKhoan() => View();

        public IActionResult ThongKeHocSinh() => View();
        public IActionResult ThongKeGiaoVien() => View();
        public IActionResult ThongKeDiem() => View();
    }
}