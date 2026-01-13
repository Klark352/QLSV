using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV.Entity;
using QLSV.Models;
using System.Security.Claims;

namespace QLSV.Controllers
{
    public class AuthController : Controller
    {
        private readonly QlsvContext _db;

        public AuthController(QlsvContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var tk = await _db.TaiKhoans
                .Include(x => x.HocSinh)
                    .ThenInclude(h => h!.IdLopHocNavigation)
                .Include(x => x.GiaoVien)
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == model.TenTaiKhoan);

            // Không tiết lộ user tồn tại hay không
            if (tk == null || string.IsNullOrWhiteSpace(tk.MatKhau))
            {
                ModelState.AddModelError("", "Sai tên tài khoản hoặc mật khẩu.");
                return View(model);
            }

            // ===== CHẶN TÀI KHOẢN BỊ KHÓA =====
            // 1 = hoạt động, 0 = bị khóa
            if (tk.TrangThai == 0)
            {
                ModelState.AddModelError("", "Bạn không thể đăng nhập vì tài khoản bị khóa.");
                return View(model);
            }

            // ===== Verify BCrypt (HASH-ONLY) =====
            var storedHash = tk.MatKhau.Trim();
            var storedHashNormalized = storedHash.StartsWith("$2y$")
                ? "$2a$" + storedHash.Substring(4)
                : storedHash;

            bool ok;
            try
            {
                ok = BCrypt.Net.BCrypt.Verify(model.MatKhau, storedHashNormalized);
            }
            catch
            {
                ok = false;
            }

            if (!ok)
            {
                ModelState.AddModelError("", "Sai tên tài khoản hoặc mật khẩu.");
                return View(model);
            }

            // ===== Claims =====
            var role = (tk.PhanQuyen ?? "HocSinh").Trim();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tk.IdTaiKhoan.ToString()),
                new Claim(ClaimTypes.Name, tk.TenTaiKhoan),
                new Claim(ClaimTypes.Role, role),

                // (tuỳ chọn) lưu trạng thái để hiển thị/kiểm tra nhanh
                new Claim("TrangThai", tk.TrangThai.ToString())
            };

            if (tk.HocSinh != null)
            {
                claims.Add(new Claim("TenHocSinh", tk.HocSinh.TenHocSinh ?? ""));
                claims.Add(new Claim("IdHocSinh", tk.HocSinh.IdHocSinh.ToString()));
                claims.Add(new Claim("TenLop", tk.HocSinh.IdLopHocNavigation?.TenLopHoc ?? ""));
            }

            if (tk.GiaoVien != null)
            {
                claims.Add(new Claim("TenGiaoVien", tk.GiaoVien.TenGiaoVien ?? ""));
                claims.Add(new Claim("IdGiaoVien", tk.GiaoVien.IdGiaoVien.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.GhiNho,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            // ===== Redirect =====
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Admin");

            if (role.Equals("GiaoVien", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "GiaoVien");

            return RedirectToAction("Dashboard", "HocSinh");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Bạn không có quyền truy cập chức năng này.");
        }
    }
}
