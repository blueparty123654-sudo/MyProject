using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data;

public partial class MyBookstoreDbContext : DbContext
{
    public MyBookstoreDbContext(DbContextOptions<MyBookstoreDbContext> options) : base(options) { }

    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<Discount> Discounts { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Giveaway> Giveaways { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // เราจะเขียน "คู่มือ" เฉพาะส่วนที่จำเป็นจริงๆ เท่านั้น
        // ในที่นี้คือการกำหนด Composite Key ให้กับตาราง Favorite
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new { f.UserId, f.ProductId });
    }
}