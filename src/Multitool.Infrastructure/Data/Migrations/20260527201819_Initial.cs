using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
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
                name: "custom_tables",
                columns: table => new
                {
                    table_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_tables", x => x.table_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_title = table.Column<string>(type: "text", nullable: false),
                    event_note = table.Column<string>(type: "text", nullable: true),
                    start_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_all_day = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    recurrence_rule = table.Column<string>(type: "text", nullable: true),
                    recurrence_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_events", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_calendar_events_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_columns",
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
                    table.PrimaryKey("pk_custom_columns", x => x.column_id);
                    table.ForeignKey(
                        name: "fk_custom_columns_table_id",
                        column: x => x.table_id,
                        principalTable: "custom_tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_rows",
                columns: table => new
                {
                    row_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    row_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_rows", x => x.row_id);
                    table.ForeignKey(
                        name: "fk_custom_rows_table_id",
                        column: x => x.table_id,
                        principalTable: "custom_tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_cells",
                columns: table => new
                {
                    row_id = table.Column<long>(type: "bigint", nullable: false),
                    column_id = table.Column<long>(type: "bigint", nullable: false),
                    val_string = table.Column<string>(type: "text", nullable: true),
                    val_int = table.Column<long>(type: "bigint", nullable: true),
                    val_dec = table.Column<decimal>(type: "numeric", nullable: true),
                    val_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    val_bool = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_cells", x => new { x.row_id, x.column_id });
                    table.ForeignKey(
                        name: "fk_custom_cells_column_id",
                        column: x => x.column_id,
                        principalTable: "custom_columns",
                        principalColumn: "column_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_cells_row_id",
                        column: x => x.row_id,
                        principalTable: "custom_rows",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_events_category_id",
                table: "calendar_events",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_cell_date",
                table: "custom_cells",
                columns: new[] { "column_id", "val_date" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_dec",
                table: "custom_cells",
                columns: new[] { "column_id", "val_dec" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_int",
                table: "custom_cells",
                columns: new[] { "column_id", "val_int" });

            migrationBuilder.CreateIndex(
                name: "ix_custom_columns_table_id",
                table: "custom_columns",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_rows_table_id",
                table: "custom_rows",
                column: "table_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_events");

            migrationBuilder.DropTable(
                name: "custom_cells");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "custom_columns");

            migrationBuilder.DropTable(
                name: "custom_rows");

            migrationBuilder.DropTable(
                name: "custom_tables");
        }
    }
}
