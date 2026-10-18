using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkDayDateUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_work_days_date",
                schema: "worktime",
                table: "work_days",
                column: "date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_work_days_date",
                schema: "worktime",
                table: "work_days");
        }
    }
}
