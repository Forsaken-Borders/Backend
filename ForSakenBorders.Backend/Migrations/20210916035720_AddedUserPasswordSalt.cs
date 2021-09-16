using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForSakenBorders.Backend.Migrations
{
    public partial class AddedUserPasswordSalt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "password_salt",
                table: "users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_time",
                table: "logs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "endpoint",
                table: "logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "token",
                table: "logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "logs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_salt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "date_time",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "endpoint",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "token",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "logs");
        }
    }
}
