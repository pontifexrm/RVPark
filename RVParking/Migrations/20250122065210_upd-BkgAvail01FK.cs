using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVParking.Migrations
{
    /// <inheritdoc />
    public partial class updBkgAvail01FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Availabilities_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_bkg_Availabilities_Bkg_PropertyPropertyId",
                table: "bkg_Availabilities");

            migrationBuilder.DropColumn(
                name: "Bkg_PropertyPropertyId",
                table: "bkg_Availabilities");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "bkg_Availabilities",
                newName: "Bkg_PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_bkg_Availabilities_Bkg_PropertyId",
                table: "bkg_Availabilities",
                column: "Bkg_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Availabilities_bkg_Properties_Bkg_PropertyId",
                table: "bkg_Availabilities",
                column: "Bkg_PropertyId",
                principalTable: "bkg_Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bkg_Availabilities_bkg_Properties_Bkg_PropertyId",
                table: "bkg_Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_bkg_Availabilities_Bkg_PropertyId",
                table: "bkg_Availabilities");

            migrationBuilder.RenameColumn(
                name: "Bkg_PropertyId",
                table: "bkg_Availabilities",
                newName: "PropertyId");

            migrationBuilder.AddColumn<int>(
                name: "Bkg_PropertyPropertyId",
                table: "bkg_Availabilities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bkg_Availabilities_Bkg_PropertyPropertyId",
                table: "bkg_Availabilities",
                column: "Bkg_PropertyPropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_bkg_Availabilities_bkg_Properties_Bkg_PropertyPropertyId",
                table: "bkg_Availabilities",
                column: "Bkg_PropertyPropertyId",
                principalTable: "bkg_Properties",
                principalColumn: "PropertyId");
        }
    }
}
