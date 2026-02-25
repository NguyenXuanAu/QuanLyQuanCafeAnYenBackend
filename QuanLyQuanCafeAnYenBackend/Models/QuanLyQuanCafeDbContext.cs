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

    public virtual DbSet<HinhAnhBan> HinhAnhBans { get; set; }

    public virtual DbSet<HinhAnhDanhGium> HinhAnhDanhGia { get; set; }

    public virtual DbSet<HinhAnhDanhMuc> HinhAnhDanhMucs { get; set; }

    public virtual DbSet<HinhAnhMonAn> HinhAnhMonAns { get; set; }

    public virtual DbSet<HinhAnhTang> HinhAnhTangs { get; set; }

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
        => optionsBuilder.UseSqlServer("Data Source=HAILONGKID\\LONG;Initial Catalog=QuanLyQuanCafeDb;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__Ban__3520ED6CE7D8C831");

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
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7BE5F360F0");

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
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietD__CDF0A114BAA5DB80");

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
                .HasConstraintName("FK__ChiTietDo__MaDon__797309D9");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaMon__7A672E12");

            entity.HasOne(d => d.MaNhanVienPhaChếNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaNhanVienPhaChế)
                .HasConstraintName("FK__ChiTietDo__MaNha__7B5B524B");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietH__CDF0A114F68252BF");

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
                .HasConstraintName("FK__ChiTietHo__MaHoa__09A971A2");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHo__MaMon__0A9D95DB");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BFEFBFC1A5");

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
                .HasConstraintName("FK__DanhGia__MaHoaDo__1AD3FDA4");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DanhGia__MaNguoi__1BC821DD");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMuc__B37508876A4DDC2B");

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
            entity.HasKey(e => e.MaDonDat).HasName("PK__DonDatBa__CD361BACF2CDA7E7");

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
                .HasConstraintName("FK__DonDatBan__MaBan__6D0D32F4");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonDatBan__MaNgu__6C190EBB");

            entity.HasOne(d => d.MaNhanVienXuLyNavigation).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.MaNhanVienXuLy)
                .HasConstraintName("FK__DonDatBan__MaNha__6E01572D");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADAF30EDB2");

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
                .HasConstraintName("FK__DonHang__MaBan__72C60C4A");

            entity.HasOne(d => d.MaDonDatNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaDonDat)
                .HasConstraintName("FK__DonHang__MaDonDa__74AE54BC");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonHang__MaNguoi__73BA3083");

            entity.HasOne(d => d.MaNhanVienNhanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVienNhan)
                .HasConstraintName("FK__DonHang__MaNhanV__75A278F5");
        });

        modelBuilder.Entity<HinhAnhBan>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhB__A9C37A9BE11960E4");

            entity.ToTable("HinhAnhBan");

            entity.HasIndex(e => e.MaBan, "idx_HinhAnhBan_MaBan");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValue(false);
            entity.Property(e => e.MaBan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.HinhAnhBans)
                .HasForeignKey(d => d.MaBan)
                .HasConstraintName("FK__HinhAnhBa__MaBan__6383C8BA");
        });

        modelBuilder.Entity<HinhAnhDanhGium>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhD__A9C37A9B34FD161F");

            entity.HasIndex(e => e.MaDanhGia, "idx_HinhAnhDanhGia_MaDanhGia");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.MaDanhGia)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.MaDanhGiaNavigation).WithMany(p => p.HinhAnhDanhGia)
                .HasForeignKey(d => d.MaDanhGia)
                .HasConstraintName("FK__HinhAnhDa__MaDan__1F98B2C1");
        });

        modelBuilder.Entity<HinhAnhDanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhD__A9C37A9BC0D4571E");

            entity.ToTable("HinhAnhDanhMuc");

            entity.HasIndex(e => e.MaDanhMuc, "idx_HinhAnhDanhMuc_MaDanhMuc");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValue(false);
            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.HinhAnhDanhMucs)
                .HasForeignKey(d => d.MaDanhMuc)
                .HasConstraintName("FK__HinhAnhDa__MaDan__5535A963");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhM__A9C37A9BDE76401E");

            entity.ToTable("HinhAnhMonAn");

            entity.HasIndex(e => e.MaMonAn, "idx_HinhAnhMonAn_MaMonAn");

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
                .HasConstraintName("FK__HinhAnhMo__MaMon__59FA5E80");
        });

        modelBuilder.Entity<HinhAnhTang>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhT__A9C37A9B49533D71");

            entity.ToTable("HinhAnhTang");

            entity.HasIndex(e => e.MaTang, "idx_HinhAnhTang_MaTang");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChuThich).HasMaxLength(200);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValue(false);
            entity.Property(e => e.MaTang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);

            entity.HasOne(d => d.MaTangNavigation).WithMany(p => p.HinhAnhTangs)
                .HasForeignKey(d => d.MaTang)
                .HasConstraintName("FK__HinhAnhTa__MaTan__5EBF139D");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDon__835ED13B448C6ACF");

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
                .HasConstraintName("FK__HoaDon__MaBan__03F0984C");

            entity.HasOne(d => d.MaDonDatNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaDonDat)
                .HasConstraintName("FK__HoaDon__MaDonDat__04E4BC85");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__HoaDon__MaNguoiD__05D8E0BE");

            entity.HasOne(d => d.MaNhanVienThuNganNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNhanVienThuNgan)
                .HasConstraintName("FK__HoaDon__MaNhanVi__06CD04F7");
        });

        modelBuilder.Entity<LichSuDiem>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__LichSuDi__0A2A24EB267866D1");

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
                .HasConstraintName("FK__LichSuDie__MaHoa__245D67DE");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuDiems)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDie__MaNgu__236943A5");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B1171625AFCC1BF3");

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
            entity.HasKey(e => e.MaLuotMuon).HasName("PK__MuonThie__903F6D172458570C");

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
                .HasConstraintName("FK__MuonThiet__MaBan__0F624AF8");

            entity.HasOne(d => d.MaNhanVienChoMuonNavigation).WithMany(p => p.MuonThietBis)
                .HasForeignKey(d => d.MaNhanVienChoMuon)
                .HasConstraintName("FK__MuonThiet__MaNha__10566F31");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762DD4FAB7B");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NguoiDun__0389B7BDA263B40D").IsUnique();

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
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA47C9EDD71D");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NhanVien__0389B7BD994A7397").IsUnique();

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
            entity.HasKey(e => e.MaPhanCong).HasName("PK__PhanCong__C279D91618599EF1");

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
                .HasConstraintName("FK__PhanCong__MaCa__2A164134");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhanCongs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanCong__MaNhan__29221CFB");
        });

        modelBuilder.Entity<Tang>(entity =>
        {
            entity.HasKey(e => e.MaTang).HasName("PK__Tang__9D73A49D0FD13FD7");

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
            entity.HasKey(e => e.MaYeuCau).HasName("PK__YeuCauNh__CFA5DF4E0C4D7C83");

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
                .HasConstraintName("FK__YeuCauNha__MaBan__160F4887");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
