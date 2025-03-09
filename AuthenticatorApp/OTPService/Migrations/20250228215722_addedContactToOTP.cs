using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTPService.Migrations
{
    /// <inheritdoc />
    public partial class addedContactToOTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "records",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "records");
        }
    }
}
