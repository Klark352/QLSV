using System;
using System.ComponentModel.DataAnnotations;

namespace QLSV.Models
{
    public class TeacherVM
    {
        public int? Id { get; set; }

        [Required, StringLength(100)]
        public string TenGiaoVien { get; set; } = "";

        [Required, StringLength(10)]
        public string GioiTinh { get; set; } = "Nam"; // Nam/Nu/Khac

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; } = DateTime.Today.AddYears(-25);

        [StringLength(20)]
        [RegularExpression(@"^[0-9+\-() ]*$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
    }
}
