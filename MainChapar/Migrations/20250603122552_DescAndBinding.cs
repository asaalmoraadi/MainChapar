using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class DescAndBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BindingType",
                table: "ColorPrintDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ColorPrintDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilesJson",
                table: "ColorPrintDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPages",
                table: "ColorPrintDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BindingType",
                table: "BlackWhitePrintDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BlackWhitePrintDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BindingType",
                table: "ColorPrintDetails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ColorPrintDetails");

            migrationBuilder.DropColumn(
                name: "FilesJson",
                table: "ColorPrintDetails");

            migrationBuilder.DropColumn(
                name: "TotalPages",
                table: "ColorPrintDetails");

            migrationBuilder.DropColumn(
                name: "BindingType",
                table: "BlackWhitePrintDetails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "BlackWhitePrintDetails");
        }
    }
}
