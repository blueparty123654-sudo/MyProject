using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRedemptionPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Giveaways_GiveawayId",
                table: "Redemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Users_UserId",
                table: "Redemptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Redemptions",
                table: "Redemptions");

            migrationBuilder.AddColumn<int>(
                name: "RedemptionId",
                table: "Redemptions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Redemptions",
                table: "Redemptions",
                column: "RedemptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_UserId",
                table: "Redemptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Giveaways_GiveawayId",
                table: "Redemptions",
                column: "GiveawayId",
                principalTable: "Giveaways",
                principalColumn: "GiveawayId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Users_UserId",
                table: "Redemptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Giveaways_GiveawayId",
                table: "Redemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Users_UserId",
                table: "Redemptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Redemptions",
                table: "Redemptions");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_UserId",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "RedemptionId",
                table: "Redemptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Redemptions",
                table: "Redemptions",
                columns: new[] { "UserId", "GiveawayId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Giveaways_GiveawayId",
                table: "Redemptions",
                column: "GiveawayId",
                principalTable: "Giveaways",
                principalColumn: "GiveawayId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Users_UserId",
                table: "Redemptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
