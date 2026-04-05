using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntercityTransportManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirationTimeToReservationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusSeats_BusId",
                table: "BusSeats");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationTime",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TravelDate",
                table: "BusSchedule",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_BusSeats_BusId_Number",
                table: "BusSeats",
                columns: new[] { "BusId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusSeats_BusId_Number",
                table: "BusSeats");

            migrationBuilder.DropColumn(
                name: "ExpirationTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TravelDate",
                table: "BusSchedule");

            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "Reservations",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_BusSeats_BusId",
                table: "BusSeats",
                column: "BusId");
        }
    }
}
