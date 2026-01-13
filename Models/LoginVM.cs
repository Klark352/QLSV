using System.ComponentModel.DataAnnotations;

namespace QLSV.Models
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        public string TenTaiKhoan { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string MatKhau { get; set; } = "";

        public bool GhiNho { get; set; } = true;
    }
}
