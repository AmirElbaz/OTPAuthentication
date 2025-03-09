using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTPService.Migrations
{
    /// <inheritdoc />
    public partial class OTPprops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "records",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "records",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "records",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StepId",
                table: "records",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "records",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValidationAttempts",
                table: "records",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "records");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "records");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "records");

            migrationBuilder.DropColumn(
                name: "StepId",
                table: "records");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "records");

            migrationBuilder.DropColumn(
                name: "ValidationAttempts",
                table: "records");
        }
    }
}
