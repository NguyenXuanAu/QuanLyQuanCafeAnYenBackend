# ☕ Quản Lý Quán Cafe An Yên

Website quản lý quán cafe với đặt bàn online, order realtime, tích điểm thành viên.

## 🎯 Tính năng

### Khách hàng
- Đặt bàn online qua QR Code
- Order món tại bàn
- Vote nhạc yêu thích
- Tích điểm thành viên

### Nhân viên
- Quản lý đơn hàng realtime
- Xử lý order bếp
- Thu ngân nhanh
- Xem sơ đồ bàn

### Quản lý
- Dashboard thống kê
- Quản lý nhân viên & menu
- Báo cáo doanh thu

## 🛠️ Công nghệ

- ASP.NET Core 8.0 Web API
- SQL Server
- Entity Framework Core
- JWT Authentication
- SignalR (Realtime)

## 📦 Hướng dẫn cài đặt cho Team

### 1. Clone project
```bash
git clone https://github.com/YOUR_USERNAME/QuanLyQuanCafeAnYen.git
cd QuanLyQuanCafeAnYen
```

### 2. Cấu hình Database

**Bước 1:** Copy file cấu hình
- Tìm file `appsettings.Example.json`
- Copy và đổi tên thành `appsettings.Development.json`

**Bước 2:** Sửa connection string trong file vừa tạo
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QuanLyQuanCafeDb;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

Thay `YOUR_SERVER` bằng tên SQL Server của bạn (xem trong SSMS)

**Bước 3:** Tạo database
- Mở SQL Server Management Studio
- Chạy script tạo database (nếu có file .sql)

### 3. Chạy project
```bash
dotnet restore
dotnet run
```

Truy cập: https://localhost:7xxx/swagger

## ⚠️ LƯU Ý BẢO MẬT

**TUYỆT ĐỐI KHÔNG** commit những file sau:
- `appsettings.Development.json` (có mật khẩu database)
- `appsettings.Production.json`
- Bất kỳ file có API keys, secrets

File `.gitignore` đã được cấu hình để bỏ qua các file này.

## 🤝 Quy tắc làm việc nhóm

### Trước khi code (mỗi sáng)
```bash
git pull origin main
```

### Tạo branch cho tính năng mới
```bash
git checkout -b feature/ten-tinh-nang
```

### Sau khi code xong
```bash
git add .
git commit -m "feat: Mô tả tính năng đã làm"
git push origin feature/ten-tinh-nang
```

### Tạo Pull Request
1. Vào GitHub repository
2. Click nút "Compare & pull request"
3. Viết mô tả chi tiết
4. Đợi team review code
5. Sau khi approve → Merge

## 🔀 Branch Strategy

- `main` - Code production, luôn stable
- `develop` - Code đang phát triển
- `feature/*` - Tính năng mới
- `bugfix/*` - Sửa lỗi

## 👨‍💻 Team Members

- [Tên bạn] - Team Lead / Backend Developer
- [Thành viên 2] - Backend Developer
- [Thành viên 3] - Database Designer
- [Thành viên 4] - Tester

## 📞 Liên hệ

- Email: your-email@example.com
- GitHub: https://github.com/YOUR_USERNAME

---
Made with ☕ by Team Quan Cafe An Yen
```

5. **Lưu và đóng Notepad** (Ctrl + S)

---

## ✅ **BƯỚC 3: KIỂM TRA ĐÃ ĐỦ FILE CHƯA**

Bây giờ kiểm tra lại cấu trúc:

**Thư mục gốc** (nơi có file `.sln`):
```
✅ .gitignore
✅ README.md                      ← Vừa tạo
✅ QuanLyQuanCafeAnYenBackend.sln
📁 QuanLyQuanCafeAnYenBackend/
```

**Thư mục con** `QuanLyQuanCafeAnYenBackend/`:
```
✅ appsettings.json
✅ appsettings.Example.json       ← Vừa tạo
✅ Program.cs
📁 Controllers/
📁 Models/
```

---

## 🚀 **BƯỚC 4: MỞ VISUAL STUDIO VÀ ĐĂNG NHẬP GITHUB**

1. **Mở Visual Studio:**
   - Double click vào file `QuanLyQuanCafeAnYenBackend.sln`
   - Đợi Visual Studio mở project

2. **Đăng nhập GitHub trong Visual Studio:**
   
   **Cách 1:**
   - Click vào biểu tượng **người dùng** ở góc trên phải Visual Studio
   - Click **"Sign in"**
   - Chọn **"Sign in with GitHub"**
   - Trình duyệt mở → Đăng nhập GitHub
   - Cho phép Visual Studio truy cập
   - Đợi kết nối xong

   **Cách 2:**
   - Menu **File** → **Account Settings**
   - Click **"Sign in"**
   - Làm tương tự bên trên

---

## 📤 **BƯỚC 5: TẠO GIT REPOSITORY VÀ PUSH LÊN GITHUB**

### **Cách 1: Dùng Visual Studio (Đơn giản nhất - Khuyên dùng)**

1. **Tìm nút "Add to Source Control":**
   - Nhìn xuống **góc dưới bên phải** màn hình Visual Studio
   - Bạn sẽ thấy chữ **"Add to Source Control"** (hoặc biểu tượng cái phích cắm)

2. **Khởi tạo Git:**
   - Click vào **mũi tên nhỏ** bên cạnh "Add to Source Control"
   - Chọn **"Git"**

   **HOẶC**

   - Menu **Git** (trên thanh menu) → **Create Git Repository**

3. **Cửa sổ "Create a Git repository" hiện ra:**

   Có 2 tab: **GitHub** và **Local**
   
   **Chọn tab "GitHub":**
```
   GitHub account: [Hiện tên GitHub của bạn]
   Owner: [Chọn tên GitHub của bạn]
   Repository name: QuanLyQuanCafeAnYen
   Description: Website quản lý quán cafe An Yên - ASP.NET Core API
   
   Visibility:
     ○ Public
     ● Private          ← Chọn cái này (cho làm việc nhóm)
   
   Initialize this repository with:
     ☐ Add a README    ← BỎ TICK (đã tạo rồi)
     ☐ Add a .gitignore ← BỎ TICK (đã tạo rồi)