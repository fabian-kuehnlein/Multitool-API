using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultitoolApi.Migrations
{
    /// <inheritdoc />
    public partial class V11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custom_table",
                columns: table => new
                {
                    tableId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tableName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_table", x => x.tableId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "custom_column",
                columns: table => new
                {
                    ColumnId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TableId = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_type = table.Column<string>(type: "enum('string','int','decimal','date','bool')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    col_order = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "custom_row",
                columns: table => new
                {
                    RowId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TableId = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "custom_cell",
                columns: table => new
                {
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    ColumnId = table.Column<long>(type: "bigint", nullable: false),
                    val_string = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    val_int = table.Column<long>(type: "bigint", nullable: true),
                    val_dec = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    val_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    val_bool = table.Column<bool>(type: "tinyint(1)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "custom_cell");

            migrationBuilder.DropTable(
                name: "custom_column");

            migrationBuilder.DropTable(
                name: "custom_row");

            migrationBuilder.DropTable(
                name: "custom_table");
        }
    }
}
