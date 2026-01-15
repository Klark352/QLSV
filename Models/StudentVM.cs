using System;
using System.ComponentModel.DataAnnotations;

namespace QLSV.Models
{
    public class StudentVM
    {
        public int? Id { get; set; }

        [Required, StringLength(100)]
        public string TenHocSinh { get; set; } = "";

        [Required, StringLength(10)]
        public string GioiTinh { get; set; } = "Nam"; // Nam/Nu/Khac

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? IdLopHoc { get; set; }
    }
}
