using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVParking.Migrations
{
    /// <inheritdoc />
    public partial class AppParam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParamKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParamValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ParamDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ParamType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppParameters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppParameters_ParamKey",
                table: "AppParameters",
                column: "ParamKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppParameters");
        }
    }
}
