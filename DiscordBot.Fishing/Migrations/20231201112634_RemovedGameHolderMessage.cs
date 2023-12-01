using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Features.Fishing.Migrations
{
    /// <inheritdoc />
    public partial class RemovedGameHolderMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameHolderMessage",
                table: "GameStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameHolderMessage",
                table: "GameStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
