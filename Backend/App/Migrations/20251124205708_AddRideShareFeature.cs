using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gemeinschaftsgipfel.Migrations
{
    /// <inheritdoc />
    public partial class AddRideShareFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RideShares",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    AvailableSeats = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<string>(type: "TEXT", nullable: false),
                    To = table.Column<string>(type: "TEXT", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Stops = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideShares_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RideShareComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<string>(type: "TEXT", nullable: true),
                    RideShareId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideShareComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideShareComments_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RideShareComments_RideShares_RideShareId",
                        column: x => x.RideShareId,
                        principalTable: "RideShares",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RideShareReservations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    RideShareId = table.Column<string>(type: "TEXT", nullable: false),
                    PassengerId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideShareReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideShareReservations_AspNetUsers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RideShareReservations_RideShares_RideShareId",
                        column: x => x.RideShareId,
                        principalTable: "RideShares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RideShareComments_CreatorId",
                table: "RideShareComments",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RideShareComments_RideShareId",
                table: "RideShareComments",
                column: "RideShareId");

            migrationBuilder.CreateIndex(
                name: "IX_RideShareReservations_PassengerId",
                table: "RideShareReservations",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_RideShareReservations_RideShareId",
                table: "RideShareReservations",
                column: "RideShareId");

            migrationBuilder.CreateIndex(
                name: "IX_RideShares_DriverId",
                table: "RideShares",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RideShareComments");

            migrationBuilder.DropTable(
                name: "RideShareReservations");

            migrationBuilder.DropTable(
                name: "RideShares");
        }
    }
}
