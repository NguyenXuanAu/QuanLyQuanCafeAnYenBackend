using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafeAnYenBackend.DTOs
{
    // 1. DTO Đăng ký
    public class RegisterDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(0[3|5|7|8|9])([0-9]{8})$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        public string Age { get; set; } = "adult";
    }
    public class LoginDto
    {
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại hoặc Email")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }
    
}