using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainChapar.Migrations
{
    /// <inheritdoc />
    public partial class AddLaminate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "laminateDetails",
                columns: table => new
                {
                    PrintRequestId = table.Column<int>(type: "int", nullable: false),
                    PaperType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintSide = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    printType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CopyCount = table.Column<int>(type: "int", nullable: false),
                    TotalPages = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaminateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CornerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laminateDetails", x => x.PrintRequestId);
                    table.ForeignKey(
                        name: "FK_laminateDetails_PrintRequests_PrintRequestId",
                        column: x => x.PrintRequestId,
                        principalTable: "PrintRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "laminatePrintPricings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDoubleSided = table.Column<bool>(type: "bit", nullable: false),
                    PricePerPage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    PrintType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaminateType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laminatePrintPricings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "laminateDetails");

            migrationBuilder.DropTable(
                name: "laminatePrintPricings");
        }
    }
}
