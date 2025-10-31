using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeProductTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. สร้างตารางใหม่ก่อน ---
            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    GearType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoolingSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartingSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuelDispensing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuelTankCapacity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrakeSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suspension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TireSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleWeight = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_ProductDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ProductImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.ProductImageId);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // --- 2. ย้ายข้อมูลจากตาราง Products เก่า ไปตารางใหม่ (ก่อนลบ) ---
            migrationBuilder.Sql(@"
                INSERT INTO ProductDetails (ProductId, GearType, Engine, CoolingSystem, StartingSystem, FuelType, FuelDispensing, FuelTankCapacity, BrakeSystem, Suspension, TireSize, Dimensions, VehicleWeight)
                SELECT
                    ProductId,
                    GearType,
                    Engine,
                    CoolingSystem,
                    StartingSystem,
                    FuelType,
                    FuelDispensing,
                    FuelTankCapacity,
                    BrakeSystem,
                    Suspension,
                    TireSize,
                    Dimensions,
                    VehicleWeight
                FROM Products
                WHERE ProductId IS NOT NULL
            ");

            // --- 2.2 ย้าย ImageUrl (รูปแรก ให้เป็น IsPrimary = 1 (true)) ---
            migrationBuilder.Sql(@"
                INSERT INTO ProductImages (ProductId, Url, IsPrimary)
                SELECT ProductId, ImageUrl, 1
                FROM Products
                WHERE ImageUrl IS NOT NULL AND ImageUrl != ''
            ");

            // --- 2.3 ย้าย ImageUrl2 (รูปสอง ให้เป็น IsPrimary = 0 (false)) ---
            migrationBuilder.Sql(@"
                INSERT INTO ProductImages (ProductId, Url, IsPrimary)
                SELECT ProductId, ImageUrl2, 0
                FROM Products
                WHERE ImageUrl2 IS NOT NULL AND ImageUrl2 != ''
            ");

            // (CreateIndex อยู่หลัง CreateTable ก็ดีแล้ว)
            migrationBuilder.CreateIndex(
               name: "IX_ProductImages_ProductId",
               table: "ProductImages",
               column: "ProductId");

            // --- 3. ลบคอลัมน์เก่า (ย้ายมาไว้ล่างสุด) ---
            migrationBuilder.DropColumn(
                name: "BrakeSystem",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CoolingSystem",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Engine",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FuelDispensing",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FuelTankCapacity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GearType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StartingSystem",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Suspension",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TireSize",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VehicleWeight",
                table: "Products");
        }
    }
}
