using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrintPricings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDoubleSided",
                table: "printPricings");

            migrationBuilder.DropColumn(
                name: "IsDoubleSided",
                table: "laminatePrintPricings");

            migrationBuilder.AddColumn<string>(
                name: "PrintSide",
                table: "printPricings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrintSide",
                table: "laminatePrintPricings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintSide",
                table: "printPricings");

            migrationBuilder.DropColumn(
                name: "PrintSide",
                table: "laminatePrintPricings");

            migrationBuilder.AddColumn<bool>(
                name: "IsDoubleSided",
                table: "printPricings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoubleSided",
                table: "laminatePrintPricings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
