using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class usedBookImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsedBookImage_UsedBook_UsedBookId",
                table: "UsedBookImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsedBookImage",
                table: "UsedBookImage");

            migrationBuilder.RenameTable(
                name: "UsedBookImage",
                newName: "usedBookImages");

            migrationBuilder.RenameIndex(
                name: "IX_UsedBookImage_UsedBookId",
                table: "usedBookImages",
                newName: "IX_usedBookImages_UsedBookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usedBookImages",
                table: "usedBookImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_usedBookImages_UsedBook_UsedBookId",
                table: "usedBookImages",
                column: "UsedBookId",
                principalTable: "UsedBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usedBookImages_UsedBook_UsedBookId",
                table: "usedBookImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usedBookImages",
                table: "usedBookImages");

            migrationBuilder.RenameTable(
                name: "usedBookImages",
                newName: "UsedBookImage");

            migrationBuilder.RenameIndex(
                name: "IX_usedBookImages_UsedBookId",
                table: "UsedBookImage",
                newName: "IX_UsedBookImage_UsedBookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsedBookImage",
                table: "UsedBookImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsedBookImage_UsedBook_UsedBookId",
                table: "UsedBookImage",
                column: "UsedBookId",
                principalTable: "UsedBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
