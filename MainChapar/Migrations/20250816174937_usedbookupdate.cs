using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class usedbookupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "UsedBook",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "UsedBook");
        }
    }
}
