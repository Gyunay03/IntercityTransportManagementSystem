using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntercityTransportManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class HashTokenForEmailAndPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordResetToken",
                table: "Users",
                newName: "PasswordResetTokenHash");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationToken",
                table: "Users",
                newName: "EmailVerificationTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordResetTokenHash",
                table: "Users",
                newName: "PasswordResetToken");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationTokenHash",
                table: "Users",
                newName: "EmailVerificationToken");
        }
    }
}
