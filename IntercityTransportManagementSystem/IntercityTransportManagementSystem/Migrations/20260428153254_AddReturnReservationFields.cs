using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntercityTransportManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnReservationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnReservationId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReturnReservationId",
                table: "Reservations",
                column: "ReturnReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Reservations_ReturnReservationId",
                table: "Reservations",
                column: "ReturnReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Reservations_ReturnReservationId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReturnReservationId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReturnReservationId",
                table: "Reservations");
        }
    }
}
