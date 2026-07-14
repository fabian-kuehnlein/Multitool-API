using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_WorkTimePlanner_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "worktime");

            migrationBuilder.CreateTable(
                name: "week_summaries",
                schema: "worktime",
                columns: table => new
                {
                    week_summary_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    year = table.Column<int>(type: "integer", nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    total_overtime = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_week_summaries", x => x.week_summary_id);
                });

            migrationBuilder.CreateTable(
                name: "work_days",
                schema: "worktime",
                columns: table => new
                {
                    work_day_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    break_minutes = table.Column<int>(type: "integer", nullable: false),
                    work_minutes = table.Column<int>(type: "integer", nullable: false),
                    overtime_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_home_office = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    warnings = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_days", x => x.work_day_id);
                });

            migrationBuilder.CreateTable(
                name: "work_time_settings",
                schema: "worktime",
                columns: table => new
                {
                    settings_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    daily_target_minutes = table.Column<int>(type: "integer", nullable: false),
                    break_rule_6h = table.Column<int>(type: "integer", nullable: false),
                    break_rule_9h = table.Column<int>(type: "integer", nullable: false),
                    home_office_limit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_time_settings", x => x.settings_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_week_summaries_year_week",
                schema: "worktime",
                table: "week_summaries",
                columns: new[] { "year", "week_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "week_summaries",
                schema: "worktime");

            migrationBuilder.DropTable(
                name: "work_days",
                schema: "worktime");

            migrationBuilder.DropTable(
                name: "work_time_settings",
                schema: "worktime");
        }
    }
}
