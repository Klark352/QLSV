using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QLSV.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: /Admin/Dashboard
        public IActionResult Dashboard() => View();

        public IActionResult HocSinh() => View();
        public IActionResult GiaoVien() => View();
        public IActionResult LopHoc() => View();
        public IActionResult MonHoc() => View();
        public IActionResult BangDiem() => View();
        public IActionResult TaiKhoan() => View();

        public IActionResult ThongKeHocSinh() => View();
        public IActionResult ThongKeGiaoVien() => View();
        public IActionResult ThongKeDiem() => View();
    }
}