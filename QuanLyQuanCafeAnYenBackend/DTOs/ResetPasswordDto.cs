using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafeAnYenBackend.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        public string Otp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}