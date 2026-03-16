namespace bandothanhli.Data
{
    using Microsoft.EntityFrameworkCore;
    using bandothanhli.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<TinhTrangSanPham> TinhTrangSanPhams { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<AnhSanPham> AnhSanPhams { get; set; }
        public DbSet<DiaChi> DiaChis { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<TheoDoidonHang> TheoDoidonHangs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }
        public DbSet<OtpXacMinh> OtpXacMinhs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DonHang - NguoiMua
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NguoiMua)
                .WithMany(n => n.DonHangs)
                .HasForeignKey(d => d.NguoiMuaId)
                .OnDelete(DeleteBehavior.Restrict);

            // DonHang - DiaChi
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.DiaChi)
                .WithMany(dc => dc.DonHangs)
                .HasForeignKey(d => d.DiaChiId)
                .OnDelete(DeleteBehavior.Restrict);

            // TinNhan - NguoiGui
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.NguoiGui)
                .WithMany()
                .HasForeignKey(t => t.NguoiGuiId)
                .OnDelete(DeleteBehavior.Restrict);

            // TinNhan - NguoiNhan
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.NguoiNhan)
                .WithMany()
                .HasForeignKey(t => t.NguoiNhanId)
                .OnDelete(DeleteBehavior.Restrict);

            // TinNhan - SanPham
            modelBuilder.Entity<TinNhan>()
                .HasOne(t => t.SanPham)
                .WithMany()
                .HasForeignKey(t => t.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            // DanhGia - NguoiDanhGia
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.NguoiDanhGia)
                .WithMany(n => n.DanhGias)
                .HasForeignKey(d => d.NguoiDanhGiaId)
                .OnDelete(DeleteBehavior.Restrict);

            // DanhGia - SanPham
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.SanPham)
                .WithMany(s => s.DanhGias)
                .HasForeignKey(d => d.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            // SanPham - NguoiBan
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NguoiBan)
                .WithMany(n => n.SanPhams)
                .HasForeignKey(s => s.NguoiBanId)
                .OnDelete(DeleteBehavior.Restrict);

            // DanhMuc tu tham chieu
            modelBuilder.Entity<DanhMuc>()
                .HasOne(d => d.DanhMucCha)
                .WithMany(d => d.DanhMucCons)
                .HasForeignKey(d => d.DanhMucChaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
