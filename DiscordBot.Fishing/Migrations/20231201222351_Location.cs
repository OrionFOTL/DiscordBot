using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Features.Fishing.Migrations
{
    /// <inheritdoc />
    public partial class Location : Migration
    {
        private static readonly string[] columns = ["Id", "Code", "Name"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLocationCode",
                table: "GameStates");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "GameStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: columns,
                values: new object[] { 1, "jp", "Japan" });

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_LocationId",
                table: "GameStates",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Code",
                table: "Locations",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameStates_Locations_LocationId",
                table: "GameStates",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameStates_Locations_LocationId",
                table: "GameStates");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_GameStates_LocationId",
                table: "GameStates");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "GameStates");

            migrationBuilder.AddColumn<string>(
                name: "CurrentLocationCode",
                table: "GameStates",
                type: "TEXT",
                nullable: true);
        }
    }
}
