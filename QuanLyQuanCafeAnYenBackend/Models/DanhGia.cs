using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanCafeAnYenBackend.Models
{
    [Table("DanhGia")]
    public partial class DanhGia
    {
        [Key]
        [StringLength(10)]
        public string MaDanhGia { get; set; } = null!;
        public int SoSao { get; set; }
        [StringLength(500)]
        public string? BinhLuan { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        [StringLength(10)]
        public string MaHoaDon { get; set; } = null!;
        [StringLength(10)]
        public string? MaNguoiDung { get; set; }
    }
}