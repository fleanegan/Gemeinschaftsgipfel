using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gemeinschaftsgipfel.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeleteForComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumComments_ForumEntries_ForumEntryId",
                table: "ForumComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RideShareComments_RideShares_RideShareId",
                table: "RideShareComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicComments_Topics_TopicId",
                table: "TopicComments");

            migrationBuilder.AlterColumn<string>(
                name: "TopicId",
                table: "TopicComments",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RideShareId",
                table: "RideShareComments",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ForumEntryId",
                table: "ForumComments",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ForumComments_ForumEntries_ForumEntryId",
                table: "ForumComments",
                column: "ForumEntryId",
                principalTable: "ForumEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RideShareComments_RideShares_RideShareId",
                table: "RideShareComments",
                column: "RideShareId",
                principalTable: "RideShares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicComments_Topics_TopicId",
                table: "TopicComments",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumComments_ForumEntries_ForumEntryId",
                table: "ForumComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RideShareComments_RideShares_RideShareId",
                table: "RideShareComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicComments_Topics_TopicId",
                table: "TopicComments");

            migrationBuilder.AlterColumn<string>(
                name: "TopicId",
                table: "TopicComments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "RideShareId",
                table: "RideShareComments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ForumEntryId",
                table: "ForumComments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumComments_ForumEntries_ForumEntryId",
                table: "ForumComments",
                column: "ForumEntryId",
                principalTable: "ForumEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RideShareComments_RideShares_RideShareId",
                table: "RideShareComments",
                column: "RideShareId",
                principalTable: "RideShares",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicComments_Topics_TopicId",
                table: "TopicComments",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");
        }
    }
}
