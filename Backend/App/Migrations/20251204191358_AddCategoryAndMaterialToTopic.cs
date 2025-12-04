using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gemeinschaftsgipfel.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndMaterialToTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Topics",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Topics",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Topics");
        }
    }
}
