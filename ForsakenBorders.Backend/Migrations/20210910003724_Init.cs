using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForsakenBorders.Backend.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ban_expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_banned = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    ban_reason = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    password_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    token = table.Column<string>(type: "text", nullable: true),
                    token_expiration = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    first_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    last_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    content_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tags = table.Column<List<string>>(type: "text[]", maxLength: 10, nullable: true),
                    thumbnail = table.Column<byte[]>(type: "bytea", nullable: true),
                    thumbnail_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notes_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_official = table.Column<bool>(type: "boolean", nullable: false),
                    icon = table.Column<byte[]>(type: "bytea", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    user_permissions = table.Column<byte>(type: "smallint", nullable: false),
                    note_permissions = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notes_owner_id",
                table: "notes",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_user_id",
                table: "roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
