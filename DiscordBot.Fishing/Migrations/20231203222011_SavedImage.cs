using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Features.Fishing.Migrations
{
    /// <inheritdoc />
    public partial class SavedImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedImages_Name",
                table: "SavedImages",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedImages");
        }
    }
}
