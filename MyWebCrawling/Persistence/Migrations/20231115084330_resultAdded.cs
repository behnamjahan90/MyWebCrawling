using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebCrawling.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class resultAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uri",
                table: "SearchJobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageCounts = table.Column<int>(type: "int", nullable: false),
                    ErrorCounts = table.Column<int>(type: "int", nullable: false),
                    JobQueue = table.Column<int>(type: "int", nullable: false),
                    ResultCount = table.Column<int>(type: "int", nullable: false),
                    UrlAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropColumn(
                name: "Uri",
                table: "SearchJobs");
        }
    }
}
