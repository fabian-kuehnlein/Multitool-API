using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoAndCategoryExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_column_id",
                table: "custom_cells");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_row_id",
                table: "custom_cells");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_rows_table_id",
                table: "custom_rows");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_columns_table_id",
                table: "custom_columns");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_tables",
                table: "custom_tables");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_rows",
                table: "custom_rows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_columns",
                table: "custom_columns");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_cells",
                table: "custom_cells");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "custom");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "categories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "calendar_events",
                newName: "calendar_events",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "custom_tables",
                newName: "tables",
                newSchema: "custom");

            migrationBuilder.RenameTable(
                name: "custom_rows",
                newName: "rows",
                newSchema: "custom");

            migrationBuilder.RenameTable(
                name: "custom_columns",
                newName: "columns",
                newSchema: "custom");

            migrationBuilder.RenameTable(
                name: "custom_cells",
                newName: "cells",
                newSchema: "custom");

            migrationBuilder.RenameIndex(
                name: "ix_custom_rows_table_id",
                schema: "custom",
                table: "rows",
                newName: "ix_rows_table_id");

            migrationBuilder.RenameIndex(
                name: "ix_custom_columns_table_id",
                schema: "custom",
                table: "columns",
                newName: "ix_columns_table_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tables",
                schema: "custom",
                table: "tables",
                column: "table_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_rows",
                schema: "custom",
                table: "rows",
                column: "row_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_columns",
                schema: "custom",
                table: "columns",
                column: "column_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cells",
                schema: "custom",
                table: "cells",
                columns: new[] { "row_id", "column_id" });

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
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creation_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "ix_todos_category_id",
                schema: "public",
                table: "todos",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_custom_columns_table_id",
                schema: "custom",
                table: "columns",
                column: "table_id",
                principalSchema: "custom",
                principalTable: "tables",
                principalColumn: "table_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_rows_table_id",
                schema: "custom",
                table: "rows",
                column: "table_id",
                principalSchema: "custom",
                principalTable: "tables",
                principalColumn: "table_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_cells_row_id",
                schema: "custom",
                table: "cells",
                column: "row_id",
                principalSchema: "custom",
                principalTable: "rows",
                principalColumn: "row_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_cells_column_id",
                schema: "custom",
                table: "cells",
                column: "column_id",
                principalSchema: "custom",
                principalTable: "columns",
                principalColumn: "column_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todos",
                schema: "public");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_columns_table_id",
                schema: "custom",
                table: "columns");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_rows_table_id",
                schema: "custom",
                table: "rows");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_row_id",
                schema: "custom",
                table: "cells");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_column_id",
                schema: "custom",
                table: "cells");

            migrationBuilder.DropPrimaryKey(name: "pk_tables", schema: "custom", table: "tables");
            migrationBuilder.DropPrimaryKey(name: "pk_rows", schema: "custom", table: "rows");
            migrationBuilder.DropPrimaryKey(name: "pk_columns", schema: "custom", table: "columns");
            migrationBuilder.DropPrimaryKey(name: "pk_cells", schema: "custom", table: "cells");

            migrationBuilder.RenameTable(name: "users", schema: "public", newName: "users");
            migrationBuilder.RenameTable(name: "categories", schema: "public", newName: "categories");
            migrationBuilder.RenameTable(name: "calendar_events", schema: "public", newName: "calendar_events");
            migrationBuilder.RenameTable(name: "tables", schema: "custom", newName: "custom_tables");
            migrationBuilder.RenameTable(name: "rows", schema: "custom", newName: "custom_rows");
            migrationBuilder.RenameTable(name: "columns", schema: "custom", newName: "custom_columns");
            migrationBuilder.RenameTable(name: "cells", schema: "custom", newName: "custom_cells");

            migrationBuilder.RenameIndex(name: "ix_rows_table_id", table: "custom_rows", newName: "ix_custom_rows_table_id");
            migrationBuilder.RenameIndex(name: "ix_columns_table_id", table: "custom_columns", newName: "ix_custom_columns_table_id");

            migrationBuilder.AddPrimaryKey(name: "pk_custom_tables", table: "custom_tables", column: "table_id");
            migrationBuilder.AddPrimaryKey(name: "pk_custom_rows", table: "custom_rows", column: "row_id");
            migrationBuilder.AddPrimaryKey(name: "pk_custom_columns", table: "custom_columns", column: "column_id");
            migrationBuilder.AddPrimaryKey(name: "pk_custom_cells", table: "custom_cells", columns: new[] { "row_id", "column_id" });

            migrationBuilder.AddForeignKey(name: "fk_custom_columns_table_id", table: "custom_columns", column: "table_id", principalTable: "custom_tables", principalColumn: "table_id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "fk_custom_rows_table_id", table: "custom_rows", column: "table_id", principalTable: "custom_tables", principalColumn: "table_id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "fk_custom_cells_row_id", table: "custom_cells", column: "row_id", principalTable: "custom_rows", principalColumn: "row_id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "fk_custom_cells_column_id", table: "custom_cells", column: "column_id", principalTable: "custom_columns", principalColumn: "column_id", onDelete: ReferentialAction.Cascade);
        }
    }
}
