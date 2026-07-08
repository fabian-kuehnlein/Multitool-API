using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedDateTimeToTodos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "completed_date_time",
                schema: "public",
                table: "todos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_date_time",
                schema: "public",
                table: "todos");
        }
    }
}
