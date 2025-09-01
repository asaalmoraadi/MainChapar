using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class UploadedFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFile_collaborations_CollaborationId",
                table: "UploadedFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UploadedFile",
                table: "UploadedFile");

            migrationBuilder.RenameTable(
                name: "UploadedFile",
                newName: "uploadedFiles");

            migrationBuilder.RenameIndex(
                name: "IX_UploadedFile_CollaborationId",
                table: "uploadedFiles",
                newName: "IX_uploadedFiles_CollaborationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_uploadedFiles",
                table: "uploadedFiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_uploadedFiles_collaborations_CollaborationId",
                table: "uploadedFiles",
                column: "CollaborationId",
                principalTable: "collaborations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_uploadedFiles_collaborations_CollaborationId",
                table: "uploadedFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_uploadedFiles",
                table: "uploadedFiles");

            migrationBuilder.RenameTable(
                name: "uploadedFiles",
                newName: "UploadedFile");

            migrationBuilder.RenameIndex(
                name: "IX_uploadedFiles_CollaborationId",
                table: "UploadedFile",
                newName: "IX_UploadedFile_CollaborationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UploadedFile",
                table: "UploadedFile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFile_collaborations_CollaborationId",
                table: "UploadedFile",
                column: "CollaborationId",
                principalTable: "collaborations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
