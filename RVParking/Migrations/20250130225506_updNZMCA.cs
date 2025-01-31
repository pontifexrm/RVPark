using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVParking.Migrations
{
    /// <inheritdoc />
    public partial class updNZMCA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserNZMCA",
                table: "bkg_Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NZMCA",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNZMCA",
                table: "bkg_Users");

            migrationBuilder.DropColumn(
                name: "NZMCA",
                table: "AspNetUsers");
        }
    }
}
