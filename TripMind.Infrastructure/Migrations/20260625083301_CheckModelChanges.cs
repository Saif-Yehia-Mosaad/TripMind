using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripMind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoritePlaces_Users_UserId",
                table: "FavoritePlaces");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritePlaces_Users_UserId",
                table: "FavoritePlaces",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoritePlaces_Users_UserId",
                table: "FavoritePlaces");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritePlaces_Users_UserId",
                table: "FavoritePlaces",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
