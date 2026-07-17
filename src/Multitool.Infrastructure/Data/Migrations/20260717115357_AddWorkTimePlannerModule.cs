using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTimePlannerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "custom");

            migrationBuilder.EnsureSchema(
                name: "worktime");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "public",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                schema: "custom",
                columns: table => new
                {
                    table_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tables", x => x.table_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

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
                    is_locked = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "calendar_events",
                schema: "public",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_title = table.Column<string>(type: "text", nullable: false),
                    event_note = table.Column<string>(type: "text", nullable: true),
                    start_date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_all_day = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    recurrence_rule = table.Column<string>(type: "text", nullable: true),
                    recurrence_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_events", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_calendar_events_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "todos",
                schema: "public",
                columns: table => new
                {
                    todo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    todo_title = table.Column<string>(type: "text", nullable: false),
                    todo_description = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    is_done = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    creation_date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    completed_date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_todos", x => x.todo_id);
                    table.ForeignKey(
                        name: "fk_todos_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "columns",
                schema: "custom",
                columns: table => new
                {
                    column_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    col_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_columns", x => x.column_id);
                    table.ForeignKey(
                        name: "fk_custom_columns_table_id",
                        column: x => x.table_id,
                        principalSchema: "custom",
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rows",
                schema: "custom",
                columns: table => new
                {
                    row_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    row_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rows", x => x.row_id);
                    table.ForeignKey(
                        name: "fk_custom_rows_table_id",
                        column: x => x.table_id,
                        principalSchema: "custom",
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cells",
                schema: "custom",
                columns: table => new
                {
                    row_id = table.Column<long>(type: "bigint", nullable: false),
                    column_id = table.Column<long>(type: "bigint", nullable: false),
                    val_string = table.Column<string>(type: "text", nullable: true),
                    val_int = table.Column<long>(type: "bigint", nullable: true),
                    val_dec = table.Column<decimal>(type: "numeric", nullable: true),
                    val_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    val_bool = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cells", x => new { x.row_id, x.column_id });
                    table.ForeignKey(
                        name: "fk_custom_cells_column_id",
                        column: x => x.column_id,
                        principalSchema: "custom",
                        principalTable: "columns",
                        principalColumn: "column_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_cells_row_id",
                        column: x => x.row_id,
                        principalSchema: "custom",
                        principalTable: "rows",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_events_category_id",
                schema: "public",
                table: "calendar_events",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_cell_date",
                schema: "custom",
                table: "cells",
                columns: new[] { "column_id", "val_date" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_dec",
                schema: "custom",
                table: "cells",
                columns: new[] { "column_id", "val_dec" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_int",
                schema: "custom",
                table: "cells",
                columns: new[] { "column_id", "val_int" });

            migrationBuilder.CreateIndex(
                name: "ix_columns_table_id",
                schema: "custom",
                table: "columns",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_rows_table_id",
                schema: "custom",
                table: "rows",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_todos_category_id",
                schema: "public",
                table: "todos",
                column: "category_id");

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
                name: "calendar_events",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cells",
                schema: "custom");

            migrationBuilder.DropTable(
                name: "todos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "week_summaries",
                schema: "worktime");

            migrationBuilder.DropTable(
                name: "work_days",
                schema: "worktime");

            migrationBuilder.DropTable(
                name: "work_time_settings",
                schema: "worktime");

            migrationBuilder.DropTable(
                name: "columns",
                schema: "custom");

            migrationBuilder.DropTable(
                name: "rows",
                schema: "custom");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tables",
                schema: "custom");
        }
    }
}
