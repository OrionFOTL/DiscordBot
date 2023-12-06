using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Features.Fishing.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentItemDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ViewingOwnedItemId",
                table: "GameStates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FishingRod",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Bait",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_ViewingOwnedItemId",
                table: "GameStates",
                column: "ViewingOwnedItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameStates_ViewingOwnedItemId",
                table: "GameStates");

            migrationBuilder.DropColumn(
                name: "ViewingOwnedItemId",
                table: "GameStates");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FishingRod");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Bait");
        }
    }
}
