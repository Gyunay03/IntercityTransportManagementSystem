using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntercityTransportManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedThreeFieldsToDriverModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Routes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "Routes",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Drivers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Drivers");
        }
    }
}
