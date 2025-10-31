using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsPrimaryWithImageNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. เพิ่มคอลัมน์ใหม่ก่อน ---
            migrationBuilder.AddColumn<int>(
                name: "ImageNo",
                table: "ProductImages",
                type: "int",
                nullable: false,
                defaultValue: 1); // ให้ค่าเริ่มต้นเป็น 1 ไปก่อน

            // --- 2. อัปเดตข้อมูล โดยอ้างอิงจาก 'IsPrimary' (ที่ยังไม่ถูกลบ) ---

            // รูปที่เป็นรูปหลัก (IsPrimary = 1) ให้ ImageNo = 1
            migrationBuilder.Sql(
                @"UPDATE ProductImages
                  SET ImageNo = 1
                  WHERE IsPrimary = 1"
            );

            // รูปที่ไม่ใช่รูปหลัก (IsPrimary = 0) ให้ ImageNo = 2
            migrationBuilder.Sql(
                @"UPDATE ProductImages
                  SET ImageNo = 2
                  WHERE IsPrimary = 0"
            );

            // --- 3. ลบคอลัมน์เก่า 'IsPrimary' (ลบครั้งเดียว ที่ล่างสุด) ---
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "ProductImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageNo",
                table: "ProductImages");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
