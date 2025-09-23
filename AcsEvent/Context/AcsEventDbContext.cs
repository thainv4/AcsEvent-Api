using AcsEvent.Entities;
using AcsEvent.Models;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Context;

public class AcsEventDbContext : DbContext
{
    public DbSet<EmployeeInfo?> EmployeeInfos { get; set; }
    public DbSet<PhongBan> PhongBans { get; set; }
    public DbSet<ThietBi> ThietBis { get; set; }
    public DbSet<CheckInOut> CheckInOuts { get; set; }
    
    public AcsEventDbContext(DbContextOptions<AcsEventDbContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.Development.json")
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configurationRoot = builder.Build();
            optionsBuilder.UseSqlServer(configurationRoot.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<EmployeeInfo>(entity =>
        {
            entity.HasKey(e => e.MaCC);
            entity.Property(e => e.MaCC).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaNV).HasMaxLength(50);
            entity.Property(e => e.MaPb).HasMaxLength(50);
        });
        
        modelBuilder.Entity<PhongBan>(entity =>
        {
            entity.HasKey(e => e.MaPb);
            entity.Property(e => e.MaPb).HasMaxLength(50);
            entity.Property(e => e.TenPb).HasMaxLength(100);
        });
        
        modelBuilder.Entity<ThietBi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenTB).HasMaxLength(100);
            entity.Property(e => e.IP).HasMaxLength(50);
            entity.Property(e => e.username).HasMaxLength(50);
            entity.Property(e => e.password).HasMaxLength(50);
        });

        modelBuilder.Entity<CheckInOut>(entity =>
        {
            entity.HasKey(e => e.MaNV);
            entity.Property(e => e.MaNV).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.TimeIn);
            entity.Property(e => e.TimeOut);
            entity.Property(e => e.DiMuon);
            entity.Property(e => e.VeSom);
        });
    }
}