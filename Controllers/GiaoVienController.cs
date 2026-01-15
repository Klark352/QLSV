using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QLSV.Entity;

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

        private void SetHeaderInfoFromClaims()
        {
            ViewBag.TenGiaoVien = User.FindFirstValue("TenGiaoVien") ?? User.Identity?.Name ?? "Giáo viên";
            ViewBag.MonHoc = User.FindFirstValue("MonHoc") ?? "";
        }

        public async Task<IActionResult> Dashboard()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult LopChuNhiem()
        {
            SetHeaderInfoFromClaims();
            return View();
        }

        public IActionResult DanhSachLop()
        {
            SetHeaderInfoFromClaims();
            return View();
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
    }
}