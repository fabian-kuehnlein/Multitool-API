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
                    categoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoryName = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.categoryId);
                });

            migrationBuilder.CreateTable(
                name: "custom_table",
                columns: table => new
                {
                    tableId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tableName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_table", x => x.tableId);
                });

            migrationBuilder.CreateTable(
                name: "calendar_events",
                columns: table => new
                {
                    eventId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eventTitle = table.Column<string>(type: "text", nullable: false),
                    eventNote = table.Column<string>(type: "text", nullable: true),
                    startDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isAllDay = table.Column<bool>(type: "boolean", nullable: false),
                    categoryId = table.Column<int>(type: "integer", nullable: false),
                    recurrenceRule = table.Column<string>(type: "text", nullable: true),
                    recurrenceEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_events", x => x.eventId);
                    table.ForeignKey(
                        name: "FK_calendar_events_categoryId",
                        column: x => x.categoryId,
                        principalTable: "categories",
                        principalColumn: "categoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_column",
                columns: table => new
                {
                    ColumnId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableId = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    col_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_column", x => x.ColumnId);
                    table.ForeignKey(
                        name: "FK_custom_column_custom_table_TableId",
                        column: x => x.TableId,
                        principalTable: "custom_table",
                        principalColumn: "tableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_row",
                columns: table => new
                {
                    RowId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableId = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RowOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_row", x => x.RowId);
                    table.ForeignKey(
                        name: "FK_custom_row_custom_table_TableId",
                        column: x => x.TableId,
                        principalTable: "custom_table",
                        principalColumn: "tableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_cell",
                columns: table => new
                {
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    ColumnId = table.Column<long>(type: "bigint", nullable: false),
                    val_string = table.Column<string>(type: "text", nullable: true),
                    val_int = table.Column<long>(type: "bigint", nullable: true),
                    val_dec = table.Column<decimal>(type: "numeric", nullable: true),
                    val_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    val_bool = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_cell", x => new { x.RowId, x.ColumnId });
                    table.ForeignKey(
                        name: "FK_custom_cell_custom_column_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "custom_column",
                        principalColumn: "ColumnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_custom_cell_custom_row_RowId",
                        column: x => x.RowId,
                        principalTable: "custom_row",
                        principalColumn: "RowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_categoryId",
                table: "calendar_events",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "idx_cell_date",
                table: "custom_cell",
                columns: new[] { "ColumnId", "val_date" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_dec",
                table: "custom_cell",
                columns: new[] { "ColumnId", "val_dec" });

            migrationBuilder.CreateIndex(
                name: "idx_cell_int",
                table: "custom_cell",
                columns: new[] { "ColumnId", "val_int" });

            migrationBuilder.CreateIndex(
                name: "IX_custom_column_TableId",
                table: "custom_column",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_custom_row_TableId",
                table: "custom_row",
                column: "TableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_events");

            migrationBuilder.DropTable(
                name: "custom_cell");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "custom_column");

            migrationBuilder.DropTable(
                name: "custom_row");

            migrationBuilder.DropTable(
                name: "custom_table");
        }
    }
}
