using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeUniqueConstraintFromPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_PhoneNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_User_PhoneNumber",
                schema: "identity",
                table: "Users",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_PhoneNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_User_PhoneNumber",
                schema: "identity",
                table: "Users",
                column: "PhoneNumber",
                unique: true);
        }
    }
}
