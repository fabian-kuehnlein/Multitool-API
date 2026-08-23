using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryApplicableModulesAndDeletedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "applicable_modules",
                schema: "public",
                table: "categories",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "applicable_modules",
                schema: "public",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "categories");
        }
    }
}
