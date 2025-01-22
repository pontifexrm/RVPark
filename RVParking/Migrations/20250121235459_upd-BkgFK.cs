using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVParking.Migrations
{
    /// <inheritdoc />
    public partial class updBkgFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Bookings_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Bookings_bkg_Users_Bkg_UserUserId",
                table: "bkg_Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "Bkg_UserUserId",
                table: "bkg_Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Bkg_PropertyPropertyId",
                table: "bkg_Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Bookings_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Bookings",
                column: "Bkg_PropertyPropertyId",
                principalTable: "bkg_Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Bookings_bkg_Users_Bkg_UserUserId",
                table: "bkg_Bookings",
                column: "Bkg_UserUserId",
                principalTable: "bkg_Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Bookings_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Bookings_bkg_Users_Bkg_UserUserId",
                table: "bkg_Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "Bkg_UserUserId",
                table: "bkg_Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Bkg_PropertyPropertyId",
                table: "bkg_Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Bookings_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Bookings",
                column: "Bkg_PropertyPropertyId",
                principalTable: "bkg_Properties",
                principalColumn: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Bookings_bkg_Users_Bkg_UserUserId",
                table: "bkg_Bookings",
                column: "Bkg_UserUserId",
                principalTable: "bkg_Users",
                principalColumn: "UserId");
        }
    }
}
