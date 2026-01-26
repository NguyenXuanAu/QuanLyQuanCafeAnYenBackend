using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class QuanLyQuanCafeDbContext : DbContext
{
    public QuanLyQuanCafeDbContext()
    {
    }

    public QuanLyQuanCafeDbContext(DbContextOptions<QuanLyQuanCafeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<CaLamViec> CaLamViecs { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DonDatBan> DonDatBans { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<HinhAnhMonAn> HinhAnhMonAns { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<LichSuDiem> LichSuDiems { get; set; }

    public virtual DbSet<MonAn> MonAns { get; set; }

    public virtual DbSet<MuonThietBi> MuonThietBis { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhanCong> PhanCongs { get; set; }

    public virtual DbSet<Tang> Tangs { get; set; }

    public virtual DbSet<YeuCauNhac> YeuCauNhacs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=QuanLyQuanCafeDb;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__Ban__3520ED6CEF17EE60");

            entity.ToTable("Ban");

            entity.HasIndex(e => e.MaTang, "idx_Ban_MaTang");

            entity.HasIndex(e => e.TrangThai, "idx_Ban_TrangThai");

            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.CoOdien)
                .HasDefaultValue(false)
                .HasColumnName("CoODien");
            entity.Property(e => e.GanCuaSo).HasDefaultValue(false);
            entity.Property(e => e.LoaiBan).HasMaxLength(50);
            entity.Property(e => e.MaTang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PhiDichVu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenBan).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaTangNavigation).WithMany(p => p.Bans)
                .HasForeignKey(d => d.MaTang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ban__MaTang__48CFD27E");
        });

        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7B34B4D8E3");

            entity.ToTable("CaLamViec");

            entity.Property(e => e.MaCa)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.TenCa).HasMaxLength(50);
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietD__CDF0A1149E7D2E6B");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaChiTiet)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.GiaTaiThoiDiem).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaMonAn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVienPhaChế)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MucDo).HasMaxLength(50);
            entity.Property(e => e.Size).HasMaxLength(10);
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHoanThanh).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiBep).HasDefaultValue(0);

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__ChiTietDo__MaDon__6B24EA82");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaMon__6C190EBB");

            entity.HasOne(d => d.MaNhanVienPhaChếNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaNhanVienPhaChế)
                .HasConstraintName("FK__ChiTietDo__MaNha__6D0D32F4");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietH__CDF0A114D70D0758");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.MaChiTiet)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.GiaTaiThoiDiemGoi).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaMonAn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__ChiTietHo__MaHoa__7B5B524B");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHo__MaMon__7C4F7684");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BFC2981616");

            entity.Property(e => e.MaDanhGia)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BinhLuan).HasMaxLength(500);
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaHoaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaHoaDo__0C85DE4D");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DanhGia__MaNguoi__0D7A0286");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMuc__B37508872FB4EBE6");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);
        });

        modelBuilder.Entity<DonDatBan>(entity =>
        {
            entity.HasKey(e => e.MaDonDat).HasName("PK__DonDatBa__CD361BACC09F4031");

            entity.ToTable("DonDatBan");

            entity.HasIndex(e => e.ThoiGianDen, "idx_DonDatBan_ThoiGianDen");

            entity.HasIndex(e => e.TrangThai, "idx_DonDatBan_TrangThai");

            entity.Property(e => e.MaDonDat)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.EmailKhachHang)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVienXuLy)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaQr)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MaQR");
            entity.Property(e => e.SdtKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenKhachHang).HasMaxLength(100);
            entity.Property(e => e.ThoiGianDen).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianNgoiDuKien).HasDefaultValue(60);
            entity.Property(e => e.ThoiGianTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TienCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienDatTruoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.MaBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDatBan__MaBan__5EBF139D");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonDatBan__MaNgu__5DCAEF64");

            entity.HasOne(d => d.MaNhanVienXuLyNavigation).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.MaNhanVienXuLy)
                .HasConstraintName("FK__DonDatBan__MaNha__5FB337D6");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD050365F1");

            entity.ToTable("DonHang");

            entity.HasIndex(e => e.TrangThai, "idx_DonHang_TrangThai");

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaDonDat)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVienNhan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThoiGianTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaBan)
                .HasConstraintName("FK__DonHang__MaBan__6477ECF3");

            entity.HasOne(d => d.MaDonDatNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaDonDat)
                .HasConstraintName("FK__DonHang__MaDonDa__66603565");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonHang__MaNguoi__656C112C");

            entity.HasOne(d => d.MaNhanVienNhanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVienNhan)
                .HasConstraintName("FK__DonHang__MaNhanV__6754599E");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhM__A9C37A9BB40AA903");

            entity.ToTable("HinhAnhMonAn");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValue(false);
            entity.Property(e => e.MaMonAn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.HinhAnhMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .HasConstraintName("FK__HinhAnhMo__MaMon__5535A963");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDon__835ED13BE68B14A8");

            entity.ToTable("HoaDon");

            entity.HasIndex(e => e.ThoiGianVao, "idx_HoaDon_ThoiGianVao");

            entity.HasIndex(e => e.TrangThai, "idx_HoaDon_TrangThai");

            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaDonDat)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVienThuNgan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.ThoiGianRa).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianVao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TienGiam)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TienPhuThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienMonAn)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienThanhToan)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__MaBan__75A278F5");

            entity.HasOne(d => d.MaDonDatNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaDonDat)
                .HasConstraintName("FK__HoaDon__MaDonDat__76969D2E");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__HoaDon__MaNguoiD__778AC167");

            entity.HasOne(d => d.MaNhanVienThuNganNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNhanVienThuNgan)
                .HasConstraintName("FK__HoaDon__MaNhanVi__787EE5A0");
        });

        modelBuilder.Entity<LichSuDiem>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__LichSuDi__0A2A24EB7052B5F0");

            entity.ToTable("LichSuDiem");

            entity.Property(e => e.MaGiaoDich)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.LichSuDiems)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__LichSuDie__MaHoa__123EB7A3");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuDiems)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDie__MaNgu__114A936A");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B117162551DEF320");

            entity.ToTable("MonAn");

            entity.Property(e => e.MaMonAn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.ConHang).HasDefaultValue(true);
            entity.Property(e => e.DonViTinh)
                .HasMaxLength(20)
                .HasDefaultValue("Ly");
            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenMonAn).HasMaxLength(100);

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.MonAns)
                .HasForeignKey(d => d.MaDanhMuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MonAn__MaDanhMuc__5070F446");
        });

        modelBuilder.Entity<MuonThietBi>(entity =>
        {
            entity.HasKey(e => e.MaLuotMuon).HasName("PK__MuonThie__903F6D175FE8DC3C");

            entity.ToTable("MuonThietBi");

            entity.Property(e => e.MaLuotMuon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVienChoMuon)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenThietBi).HasMaxLength(100);
            entity.Property(e => e.ThoiGianMuon)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianTra).HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.MuonThietBis)
                .HasForeignKey(d => d.MaBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MuonThiet__MaBan__01142BA1");

            entity.HasOne(d => d.MaNhanVienChoMuonNavigation).WithMany(p => p.MuonThietBis)
                .HasForeignKey(d => d.MaNhanVienChoMuon)
                .HasConstraintName("FK__MuonThiet__MaNha__02084FDA");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D76284CE6A53");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NguoiDun__0389B7BDCEB9DE40").IsUnique();

            entity.HasIndex(e => e.SoDienThoai, "idx_NguoiDung_SoDienThoai");

            entity.Property(e => e.MaNguoiDung)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.DiemTichLuy).HasDefaultValue(0);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA473D9A6F8B");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NhanVien__0389B7BD31B5AAA9").IsUnique();

            entity.HasIndex(e => e.SoDienThoai, "idx_NhanVien_SoDienThoai");

            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.LuongCoBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NgayVaoLam).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<PhanCong>(entity =>
        {
            entity.HasKey(e => e.MaPhanCong).HasName("PK__PhanCong__C279D916A8E78772");

            entity.ToTable("PhanCong");

            entity.Property(e => e.MaPhanCong)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaCa)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.MaCaNavigation).WithMany(p => p.PhanCongs)
                .HasForeignKey(d => d.MaCa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanCong__MaCa__17F790F9");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhanCongs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanCong__MaNhan__17036CC0");
        });

        modelBuilder.Entity<Tang>(entity =>
        {
            entity.HasKey(e => e.MaTang).HasName("PK__Tang__9D73A49D2C2A46B1");

            entity.ToTable("Tang");

            entity.Property(e => e.MaTang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.LaKhuYenTinh).HasDefaultValue(false);
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.TenTang).HasMaxLength(50);
        });

        modelBuilder.Entity<YeuCauNhac>(entity =>
        {
            entity.HasKey(e => e.MaYeuCau).HasName("PK__YeuCauNh__CFA5DF4EA7A56DBF");

            entity.ToTable("YeuCauNhac");

            entity.Property(e => e.MaYeuCau)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CaSi).HasMaxLength(100);
            entity.Property(e => e.DaPhat).HasDefaultValue(false);
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NguoiYeuCau).HasMaxLength(100);
            entity.Property(e => e.SoLuotBinhChon).HasDefaultValue(1);
            entity.Property(e => e.TenBaiHat).HasMaxLength(200);
            entity.Property(e => e.ThoiGianYeuCau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.YeuCauNhacs)
                .HasForeignKey(d => d.MaBan)
                .HasConstraintName("FK__YeuCauNha__MaBan__07C12930");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
