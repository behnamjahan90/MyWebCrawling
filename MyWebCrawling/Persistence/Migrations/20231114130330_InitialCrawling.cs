using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebCrawling.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCrawling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AbsoluteLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLinkExternalDomain = table.Column<bool>(type: "bit", nullable: false),
                    IsLinkFullyQualified = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    OriginalLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentPageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchJobs");

            migrationBuilder.DropTable(
                name: "SearchResults");
        }
    }
}
