using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gemeinschaftsgipfel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRideShareTitleAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "RideShares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "RideShares",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
