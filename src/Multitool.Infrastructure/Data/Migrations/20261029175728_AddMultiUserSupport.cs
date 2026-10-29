using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiUserSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- USERS ----------
            // The existing user with id 1 becomes the initial administrator.
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "public",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "role",
                schema: "public",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "User");

            migrationBuilder.Sql("UPDATE users SET role = 'Admin' WHERE id = 1;");

            // ---------- USER OWNERSHIP ----------
            // Added as nullable first so the existing data can be assigned to the initial admin.
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "public",
                table: "todos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "public",
                table: "categories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "public",
                table: "calendar_events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "worktime",
                table: "work_days",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "custom",
                table: "tables",
                type: "integer",
                nullable: true);

            // All existing data belongs to the initial administrator (id 1).
            migrationBuilder.Sql("""
                UPDATE public.todos SET user_id = 1;
                UPDATE public.categories SET user_id = 1;
                UPDATE public.calendar_events SET user_id = 1;
                UPDATE worktime.work_days SET user_id = 1;
                UPDATE custom.tables SET user_id = 1;
                """);

            // Afterwards user_id becomes mandatory.
            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "public",
                table: "todos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "public",
                table: "categories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "public",
                table: "calendar_events",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "worktime",
                table: "work_days",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "custom",
                table: "tables",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // ---------- INDEXES ----------
            // A work day is now unique per user and date instead of globally per date.
            migrationBuilder.DropIndex(
                name: "ux_work_days_date",
                schema: "worktime",
                table: "work_days");

            migrationBuilder.CreateIndex(
                name: "ux_work_days_user_date",
                schema: "worktime",
                table: "work_days",
                columns: new[] { "user_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_username",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_users_username",
                schema: "public",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ux_work_days_user_date",
                schema: "worktime",
                table: "work_days");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "public",
                table: "todos");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "public",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "public",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "worktime",
                table: "work_days");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "custom",
                table: "tables");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role",
                schema: "public",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ux_work_days_date",
                schema: "worktime",
                table: "work_days",
                column: "date",
                unique: true);
        }
    }
}
