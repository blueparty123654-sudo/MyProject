using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data;

public partial class MyBookstoreDbContext : DbContext
{
    public MyBookstoreDbContext(DbContextOptions<MyBookstoreDbContext> options) : base(options) { }

    // --- DbSets ทั้งหมด ---
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<BranchProduct> BranchProducts { get; set; } // 👈 (เพิ่ม)
    public virtual DbSet<Discount> Discounts { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Giveaway> Giveaways { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Redemption> Redemptions { get; set; } // 👈 (เพิ่ม)
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Favorite (Composite Key)
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new { f.UserId, f.ProductId });

        // 2. Order-Payment (One-to-One)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);

        // 3. Product-Branch (Many-to-Many)
        modelBuilder.Entity<BranchProduct>()
            .HasKey(bp => new { bp.BranchId, bp.ProductId });

        modelBuilder.Entity<BranchProduct>()
            .HasOne(bp => bp.Branch).WithMany(b => b.BranchProducts).HasForeignKey(bp => bp.BranchId);

        modelBuilder.Entity<BranchProduct>()
            .HasOne(bp => bp.Product).WithMany(p => p.BranchProducts).HasForeignKey(bp => bp.ProductId);

        // 4. User-Giveaway (Many-to-Many via Redemption)
        modelBuilder.Entity<Redemption>()
            .HasKey(r => new { r.UserId, r.GiveawayId });

        modelBuilder.Entity<Redemption>()
            .HasOne(r => r.User).WithMany(u => u.Redemptions).HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Redemption>()
            .HasOne(r => r.Giveaway).WithMany(g => g.Redemptions).HasForeignKey(r => r.GiveawayId);

        // 5. Discount-Order (One-to-Many)
        modelBuilder.Entity<Discount>()
            .HasMany(d => d.Orders)
            .WithOne(o => o.Discount)
            .HasForeignKey(o => o.DiscountId)
            .OnDelete(DeleteBehavior.SetNull); // ถ้าลบโค้ดส่วนลด, ให้ค่าใน Order เป็น null
    }
}