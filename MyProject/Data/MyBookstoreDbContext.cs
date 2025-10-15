using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data;

public partial class MyBookstoreDbContext : DbContext
{
    public MyBookstoreDbContext()
    {
    }

    public MyBookstoreDbContext(DbContextOptions<MyBookstoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Giveaway> Giveaways { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.DcId).HasName("PK__discount__33FDC9752EE58606");

            entity.ToTable("discount");

            entity.HasIndex(e => e.DcCode, "UQ__discount__1DEEF32F4DE7BAC3").IsUnique();

            entity.Property(e => e.DcId).HasColumnName("dc_id");
            entity.Property(e => e.DcCode)
                .HasMaxLength(50)
                .HasColumnName("dc_code");
            entity.Property(e => e.DcDate).HasColumnName("dc_date");
            entity.Property(e => e.DcRates)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("dc_rates");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PrId }).HasName("PK__favorite__4DC53EF77B0B3B58");

            entity.ToTable("favorite");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.FavMonthYear)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("fav_month_year");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("view_count");

            entity.HasOne(d => d.Pr).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__favorite__pr_id__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__favorite__user_i__5812160E");
        });

        modelBuilder.Entity<Giveaway>(entity =>
        {
            entity.HasKey(e => e.GId).HasName("PK__giveaway__49FB61C4AED2BE8C");

            entity.ToTable("giveaway");

            entity.Property(e => e.GId).HasColumnName("g_id");
            entity.Property(e => e.GName)
                .HasMaxLength(100)
                .HasColumnName("g_name");
            entity.Property(e => e.GPointcost).HasColumnName("g_pointcost");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OdId).HasName("PK__order__FB4B2EFE0AF710B4");

            entity.ToTable("order");

            entity.Property(e => e.OdId).HasColumnName("od_id");
            entity.Property(e => e.OdDateReceipt).HasColumnName("od_Date_receipt");
            entity.Property(e => e.OdDateReturn).HasColumnName("od_Date_Return");
            entity.Property(e => e.OdPoint).HasColumnName("od_point");
            entity.Property(e => e.OdPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("od_price");
            entity.Property(e => e.PayId).HasColumnName("pay_id");
            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.RentalType)
                .HasMaxLength(50)
                .HasColumnName("rental_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Pay).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order__pay_id__49C3F6B7");

            entity.HasOne(d => d.Pr).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order__pr_id__4AB81AF0");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order__user_id__4BAC3F29");

            entity.HasMany(d => d.Dcs).WithMany(p => p.Ods)
                .UsingEntity<Dictionary<string, object>>(
                    "Purchase",
                    r => r.HasOne<Discount>().WithMany()
                        .HasForeignKey("DcId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__purchase__dc_id__60A75C0F"),
                    l => l.HasOne<Order>().WithMany()
                        .HasForeignKey("OdId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__purchase__od_id__5FB337D6"),
                    j =>
                    {
                        j.HasKey("OdId", "DcId").HasName("PK__purchase__B874F2691DB19DB1");
                        j.ToTable("purchase");
                        j.IndexerProperty<int>("OdId").HasColumnName("od_id");
                        j.IndexerProperty<int>("DcId").HasColumnName("dc_id");
                    });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId).HasName("PK__payment__7AAD1CEADF86B847");

            entity.ToTable("payment");

            entity.Property(e => e.PayId).HasColumnName("pay_id");
            entity.Property(e => e.PayAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pay_amount");
            entity.Property(e => e.PayDate)
                .HasColumnType("datetime")
                .HasColumnName("pay_date");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(50)
                .HasColumnName("pay_method");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.PrId).HasName("PK__product__47B09F8E7B463695");

            entity.ToTable("product");

            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.PrBrake)
                .HasMaxLength(50)
                .HasColumnName("pr_brake");
            entity.Property(e => e.PrCoolin)
                .HasMaxLength(50)
                .HasColumnName("pr_coolin");
            entity.Property(e => e.PrCostdaily)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pr_costdaily");
            entity.Property(e => e.PrCostmonthly)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pr_costmonthly");
            entity.Property(e => e.PrCostweekly)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pr_costweekly");
            entity.Property(e => e.PrEngine)
                .HasMaxLength(50)
                .HasColumnName("pr_engine");
            entity.Property(e => e.PrFule)
                .HasMaxLength(50)
                .HasColumnName("pr_fule");
            entity.Property(e => e.PrFuledispensing)
                .HasMaxLength(50)
                .HasColumnName("pr_fuledispensing");
            entity.Property(e => e.PrFuletank)
                .HasMaxLength(50)
                .HasColumnName("pr_fuletank");
            entity.Property(e => e.PrGeartype)
                .HasMaxLength(50)
                .HasColumnName("pr_geartype");
            entity.Property(e => e.PrImage)
                .HasMaxLength(255)
                .HasColumnName("pr_image");
            entity.Property(e => e.PrLocation)
                .HasMaxLength(100)
                .HasColumnName("pr_location");
            entity.Property(e => e.PrName)
                .HasMaxLength(200)
                .HasColumnName("pr_name");
            entity.Property(e => e.PrSize)
                .HasMaxLength(50)
                .HasColumnName("pr_size");
            entity.Property(e => e.PrStartingsystem)
                .HasMaxLength(50)
                .HasColumnName("pr_startingsystem");
            entity.Property(e => e.PrSuspension)
                .HasMaxLength(50)
                .HasColumnName("pr_suspension");
            entity.Property(e => e.PrTriesize)
                .HasMaxLength(50)
                .HasColumnName("pr_triesize");
            entity.Property(e => e.PrVehicleweight)
                .HasMaxLength(50)
                .HasColumnName("pr_vehicleweight");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.RvId).HasName("PK__review__1EAFC214EBDC988C");

            entity.ToTable("review");

            entity.Property(e => e.RvId).HasColumnName("rv_id");
            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.RvRating).HasColumnName("rv_rating");
            entity.Property(e => e.RvText)
                .HasMaxLength(1000)
                .HasColumnName("rv_text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Pr).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PrId)
                .HasConstraintName("FK__review__pr_id__4F7CD00D");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__review__user_id__5070F446");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__user__B9BE370FED7E2C2F");

            entity.ToTable("user");

            entity.HasIndex(e => e.UserEmail, "UQ__user__B0FBA212554626AC").IsUnique();

            entity.HasIndex(e => e.UserDrivingcard, "UQ__user__BD15B1CB0FB724BF").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("In progress")
                .HasColumnName("status");
            entity.Property(e => e.UserDob).HasColumnName("user_DOB");
            entity.Property(e => e.UserDrivingcard)
                .HasMaxLength(1000)
                .HasColumnName("user_drivingcard");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("user_email");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.UserNo)
                .HasMaxLength(20)
                .HasColumnName("user_no");
            entity.Property(e => e.UserPass)
                .HasMaxLength(255)
                .HasColumnName("user_pass");
            entity.Property(e => e.UserPoint).HasColumnName("user_point");
            entity.Property(e => e.UserRole)
                .HasMaxLength(50)
                .HasDefaultValue("customer")
                .HasColumnName("user_role");

            entity.HasMany(d => d.Dcs).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Discounting",
                    r => r.HasOne<Discount>().WithMany()
                        .HasForeignKey("DcId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__discounti__dc_id__5441852A"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__discounti__user___534D60F1"),
                    j =>
                    {
                        j.HasKey("UserId", "DcId").HasName("PK__discount__FA81EB98CAA82F92");
                        j.ToTable("discounting");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("DcId").HasColumnName("dc_id");
                    });

            entity.HasMany(d => d.GIds).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Redeem",
                    r => r.HasOne<Giveaway>().WithMany()
                        .HasForeignKey("GId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__redeem__g_id__5CD6CB2B"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__redeem__user_id__5BE2A6F2"),
                    j =>
                    {
                        j.HasKey("UserId", "GId").HasName("PK__redeem__ED218113A312DEB4");
                        j.ToTable("redeem");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("GId").HasColumnName("g_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
