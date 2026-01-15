using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLSV.Entity;

public partial class QlsvContext : DbContext
{
    public QlsvContext()
    {
    }

    public QlsvContext(DbContextOptions<QlsvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BangDiem> BangDiems { get; set; }

    public virtual DbSet<GiaoVien> GiaoViens { get; set; }

    public virtual DbSet<HocSinh> HocSinhs { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<LopHocMonHoc> LopHocMonHocs { get; set; }

    public virtual DbSet<MonHoc> MonHocs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=KLARK;Initial Catalog=QLSV;Encrypt=false;Integrated Security=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BangDiem>(entity =>
        {
            entity.HasKey(e => e.IdBangDiem).HasName("PK__BangDiem__B38D5883A1420C2C");

            entity.ToTable("BangDiem", tb => tb.HasTrigger("TR_BangDiem_SetNgayCapNhat"));

            entity.Property(e => e.IdBangDiem).HasColumnName("idBangDiem");
            entity.Property(e => e.Diem15phut).HasColumnName("diem15phut");
            entity.Property(e => e.Diem45phut).HasColumnName("diem45phut");
            entity.Property(e => e.DiemHocKi).HasColumnName("diemHocKi");
            entity.Property(e => e.IdHocSinh).HasColumnName("idHocSinh");
            entity.Property(e => e.IdMonHoc).HasColumnName("idMonHoc");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");

            entity.HasOne(d => d.IdHocSinhNavigation).WithMany(p => p.BangDiems)
                .HasForeignKey(d => d.IdHocSinh)
                .HasConstraintName("FK_BangDiem_HocSinh");

            entity.HasOne(d => d.IdMonHocNavigation).WithMany(p => p.BangDiems)
                .HasForeignKey(d => d.IdMonHoc)
                .HasConstraintName("FK_BangDiem_MonHoc");
        });

        modelBuilder.Entity<GiaoVien>(entity =>
        {
            entity.HasKey(e => e.IdGiaoVien).HasName("PK__GiaoVien__76E2C01814D6CFA5");

            entity.ToTable("GiaoVien", tb => tb.HasTrigger("TR_GiaoVien_SetNgayCapNhat"));

            entity.HasIndex(e => e.IdTaiKhoan, "UX_GiaoVien_idTaiKhoan")
                .IsUnique()
                .HasFilter("([idTaiKhoan] IS NOT NULL)");

            entity.Property(e => e.IdGiaoVien).HasColumnName("idGiaoVien");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(10)
                .HasDefaultValue("Nam")
                .HasColumnName("gioiTinh");
            entity.Property(e => e.IdTaiKhoan).HasColumnName("idTaiKhoan");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgaySinh)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaySinh");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.TenGiaoVien)
                .HasMaxLength(50)
                .HasColumnName("tenGiaoVien");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithOne(p => p.GiaoVien)
                .HasForeignKey<GiaoVien>(d => d.IdTaiKhoan)
                .HasConstraintName("FK_GiaoVien_TaiKhoan");
        });

        modelBuilder.Entity<HocSinh>(entity =>
        {
            entity.HasKey(e => e.IdHocSinh).HasName("PK__HocSinh__4EFFBFB612BE3CDE");

            entity.ToTable("HocSinh", tb => tb.HasTrigger("TR_HocSinh_SetNgayCapNhat"));

            entity.HasIndex(e => e.IdTaiKhoan, "UX_HocSinh_idTaiKhoan")
                .IsUnique()
                .HasFilter("([idTaiKhoan] IS NOT NULL)");

            entity.Property(e => e.IdHocSinh).HasColumnName("idHocSinh");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(10)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.IdTaiKhoan).HasColumnName("idTaiKhoan");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgaySinh)
                .HasPrecision(0)
                .HasColumnName("ngaySinh");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TenHocSinh)
                .HasMaxLength(50)
                .HasColumnName("tenHocSinh");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.HocSinhs)
                .HasForeignKey(d => d.IdLopHoc)
                .HasConstraintName("FK_HocSinh_LopHoc");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithOne(p => p.HocSinh)
                .HasForeignKey<HocSinh>(d => d.IdTaiKhoan)
                .HasConstraintName("FK_HocSinh_TaiKhoan");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.IdLopHoc).HasName("PK__LopHoc__BB3E935AF2D5C9EF");

            entity.ToTable("LopHoc", tb => tb.HasTrigger("TR_LopHoc_SetNgayCapNhat"));

            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.IdGiaoVienChuNhiem).HasColumnName("idGiaoVienChuNhiem");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(20)
                .HasColumnName("namHoc");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TenKhoi)
                .HasMaxLength(50)
                .HasColumnName("tenKhoi");
            entity.Property(e => e.TenLopHoc)
                .HasMaxLength(50)
                .HasColumnName("tenLopHoc");

            entity.HasOne(d => d.IdGiaoVienChuNhiemNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.IdGiaoVienChuNhiem)
                .HasConstraintName("FK_LopHoc_GiaoVienChuNhiem");
        });

        modelBuilder.Entity<LopHocMonHoc>(entity =>
        {
            entity.HasKey(e => new { e.IdLopHoc, e.IdMonHoc });

            entity.ToTable("LopHoc_MonHoc", tb => tb.HasTrigger("TR_LopHoc_MonHoc_SetNgayCapNhat"));

            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.IdMonHoc).HasColumnName("idMonHoc");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.LopHocMonHocs)
                .HasForeignKey(d => d.IdLopHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_MonHoc_LopHoc");

            entity.HasOne(d => d.IdMonHocNavigation).WithMany(p => p.LopHocMonHocs)
                .HasForeignKey(d => d.IdMonHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_MonHoc_MonHoc");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.HasKey(e => e.IdMonHoc).HasName("PK__MonHoc__EC81BF91ECDE4160");

            entity.ToTable("MonHoc", tb => tb.HasTrigger("TR_MonHoc_SetNgayCapNhat"));

            entity.Property(e => e.IdMonHoc).HasColumnName("idMonHoc");
            entity.Property(e => e.IdGiaoVien).HasColumnName("idGiaoVien");
            entity.Property(e => e.KiHoc).HasColumnName("kiHoc");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(20)
                .HasColumnName("namHoc");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.SoTietHoc).HasColumnName("soTietHoc");
            entity.Property(e => e.SoTinChi)
                .HasComputedColumnSql("(case when [soTietHoc]>=(120) then (4) when [soTietHoc]>=(90) then (3) when [soTietHoc]>=(45) then (2) else (1) end)", true)
                .HasColumnName("soTinChi");
            entity.Property(e => e.TenMonHoc)
                .HasMaxLength(50)
                .HasColumnName("tenMonHoc");
            entity.Property(e => e.TenVietTat)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tenVietTat");

            entity.HasOne(d => d.IdGiaoVienNavigation).WithMany(p => p.MonHocs)
                .HasForeignKey(d => d.IdGiaoVien)
                .HasConstraintName("FK_MonHoc_GiaoVien");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.IdTaiKhoan).HasName("PK__TaiKhoan__8FA29E4A2CC61DD2");

            entity.ToTable("TaiKhoan", tb => tb.HasTrigger("TR_TaiKhoan_SetNgayCapNhat"));

            entity.HasIndex(e => e.TrangThai, "IX_TaiKhoan_trangThai");

            entity.HasIndex(e => e.TenTaiKhoan, "UX_TaiKhoan_tenTaiKhoan").IsUnique();

            entity.Property(e => e.IdTaiKhoan).HasColumnName("idTaiKhoan");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .HasColumnName("matKhau");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayCapNhat");
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.PhanQuyen)
                .HasMaxLength(50)
                .HasColumnName("phanQuyen");
            entity.Property(e => e.TenTaiKhoan)
                .HasMaxLength(50)
                .HasColumnName("tenTaiKhoan");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(1)
                .HasColumnName("trangThai");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
