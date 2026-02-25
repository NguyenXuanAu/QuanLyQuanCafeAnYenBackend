using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanCafeAnYenBackend.Models
{
    [Table("HinhAnhDanhGia")]
    public partial class HinhAnhDanhGia
    {
        [Key]
        [StringLength(20)]
        public string MaHinhAnh { get; set; } = null!;
        public string DuongDanAnh { get; set; } = null!;
        public int? ThuTuHienThi { get; set; }
        [StringLength(10)]
        public string MaDanhGia { get; set; } = null!;
        [StringLength(200)]
        public string? ChuThich { get; set; }
    }
}