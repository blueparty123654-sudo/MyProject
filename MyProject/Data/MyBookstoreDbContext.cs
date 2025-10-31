using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data;

public partial class MyBookstoreDbContext : DbContext
{
    public MyBookstoreDbContext(DbContextOptions<MyBookstoreDbContext> options) : base(options) { }

    // --- DbSets ทั้งหมด ---
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<BranchProduct> BranchProducts { get; set; }
    public virtual DbSet<Discount> Discounts { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Giveaway> Giveaways { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductDetail> ProductDetails { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<Redemption> Redemptions { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // <<< เรียก base ก่อนเสมอ

        // ----- 1. User -----
        modelBuilder.Entity<User>(entity =>
        {
            // (เพิ่ม) Email ต้องไม่ซ้ำกัน
            entity.HasIndex(u => u.Email).IsUnique();

            // (เพิ่ม) CHECK Constraint (ป้องกันค่าติดลบใน DB)
            entity.ToTable(tb => tb.HasCheckConstraint("CK_User_UserPoint", "UserPoint >= 0"));
        });

        // ----- 2. Role -----
        modelBuilder.Entity<Role>(entity =>
        {
            // (เพิ่ม) Name (ชื่อ Role) ต้องไม่ซ้ำกัน
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // ----- 3. Favorite -----
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => new { f.UserId, f.ProductId });

            // (เพิ่ม) CHECK Constraint
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Favorite_ViewCount", "ViewCount >= 0"));
        });

        // ----- 4. Order & Payment -----
        modelBuilder.Entity<Order>(entity =>
        {
            // (One-to-One)
            entity.HasOne(o => o.Payment)
                  .WithOne(p => p.Order)
                  .HasForeignKey<Payment>(p => p.OrderId);

            // (เพิ่ม) CHECK Constraints
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Order_Price", "Price >= 0"));
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Order_Point", "Point >= 0"));
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            // (เพิ่ม) CHECK Constraint
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Payment_Amount", "Amount > 0"));
        });

        // ----- 5. BranchProduct (Many-to-Many) -----
        modelBuilder.Entity<BranchProduct>(entity =>
        {
            entity.HasKey(bp => new { bp.BranchId, bp.ProductId });

            // (มีอยู่แล้ว) CHECK Constraint (StockQuantity >= 0)
            entity.ToTable(tb => tb.HasCheckConstraint("CK_BranchProduct_StockQuantity", "StockQuantity >= 0"));

            entity.HasOne(bp => bp.Branch).WithMany(b => b.BranchProducts).HasForeignKey(bp => bp.BranchId);
            entity.HasOne(bp => bp.Product).WithMany(p => p.BranchProducts).HasForeignKey(bp => bp.ProductId);
        });

        // ----- 6. Redemption -----
        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.HasKey(r => r.RedemptionId);
            entity.HasOne(r => r.User).WithMany(u => u.Redemptions).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Giveaway).WithMany(g => g.Redemptions).HasForeignKey(r => r.GiveawayId).OnDelete(DeleteBehavior.Restrict);
        });

        // ----- 7. Discount -----
        modelBuilder.Entity<Discount>(entity =>
        {
            // (มีอยู่แล้ว) Code ต้องไม่ซ้ำกัน
            entity.HasIndex(d => d.Code).IsUnique();

            entity.HasMany(d => d.Orders)
                  .WithOne(o => o.Discount)
                  .HasForeignKey(o => o.DiscountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ----- 8. Product (Relationships) -----
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.ProductDetail)
                  .WithOne(d => d.Product)
                  .HasForeignKey<ProductDetail>(d => d.ProductId);

            entity.HasMany(p => p.ProductImages)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId);
        });

        // ----- 9. ProductImage -----
        modelBuilder.Entity<ProductImage>(entity =>
        {
            // (เพิ่ม) ป้องกันไม่ให้มี (ProductId=5, ImageNo=1) ซ้ำ 2 แถว
            entity.HasIndex(i => new { i.ProductId, i.ImageNo }).IsUnique();
        });

        // ----- 10. Giveaway -----
        modelBuilder.Entity<Giveaway>(entity =>
        {
            // (เพิ่ม) CHECK Constraint
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Giveaway_PointCost", "PointCost >= 0"));
        });

        // ----- 11. Review -----
        modelBuilder.Entity<Review>(entity =>
        {
            // (เพิ่ม) CHECK Constraint (ป้องกัน Rating เพี้ยน)
            entity.ToTable(tb => tb.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));
        });
    }
}