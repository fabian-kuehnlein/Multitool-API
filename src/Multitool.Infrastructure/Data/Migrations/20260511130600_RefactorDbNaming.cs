using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDbNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calendar_events_categoryId",
                table: "calendar_events");

            migrationBuilder.DropForeignKey(
                name: "FK_custom_cell_custom_column_column_id",
                table: "custom_cell");

            migrationBuilder.DropForeignKey(
                name: "FK_custom_cell_custom_row_row_id",
                table: "custom_cell");

            migrationBuilder.DropForeignKey(
                name: "FK_custom_column_custom_table_TableId",
                table: "custom_column");

            migrationBuilder.DropForeignKey(
                name: "FK_custom_row_custom_table_TableId",
                table: "custom_row");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calendar_events",
                table: "calendar_events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_table",
                table: "custom_table");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_row",
                table: "custom_row");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_column",
                table: "custom_column");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_cell",
                table: "custom_cell");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "custom_table",
                newName: "custom_tables");

            migrationBuilder.RenameTable(
                name: "custom_row",
                newName: "custom_rows");

            migrationBuilder.RenameTable(
                name: "custom_column",
                newName: "custom_columns");

            migrationBuilder.RenameTable(
                name: "custom_cell",
                newName: "custom_cells");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "categoryName",
                table: "categories",
                newName: "category_name");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "categories",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "startDateTime",
                table: "calendar_events",
                newName: "start_date_time");

            migrationBuilder.RenameColumn(
                name: "recurrenceRule",
                table: "calendar_events",
                newName: "recurrence_rule");

            migrationBuilder.RenameColumn(
                name: "recurrenceEnd",
                table: "calendar_events",
                newName: "recurrence_end");

            migrationBuilder.RenameColumn(
                name: "isAllDay",
                table: "calendar_events",
                newName: "is_all_day");

            migrationBuilder.RenameColumn(
                name: "eventTitle",
                table: "calendar_events",
                newName: "event_title");

            migrationBuilder.RenameColumn(
                name: "eventNote",
                table: "calendar_events",
                newName: "event_note");

            migrationBuilder.RenameColumn(
                name: "endDateTime",
                table: "calendar_events",
                newName: "end_date_time");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "calendar_events",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "eventId",
                table: "calendar_events",
                newName: "event_id");

            migrationBuilder.RenameIndex(
                name: "IX_calendar_events_categoryId",
                table: "calendar_events",
                newName: "ix_calendar_events_category_id");

            migrationBuilder.RenameColumn(
                name: "tableName",
                table: "custom_tables",
                newName: "table_name");

            migrationBuilder.RenameColumn(
                name: "tableId",
                table: "custom_tables",
                newName: "table_id");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "custom_rows",
                newName: "table_id");

            migrationBuilder.RenameColumn(
                name: "RowOrder",
                table: "custom_rows",
                newName: "row_order");

            migrationBuilder.RenameColumn(
                name: "RowId",
                table: "custom_rows",
                newName: "row_id");

            migrationBuilder.RenameIndex(
                name: "IX_custom_row_TableId",
                table: "custom_rows",
                newName: "ix_custom_rows_table_id");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "custom_columns",
                newName: "table_id");

            migrationBuilder.RenameIndex(
                name: "IX_custom_column_TableId",
                table: "custom_columns",
                newName: "ix_custom_columns_table_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "custom_tables",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "custom_rows",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_categories",
                table: "categories",
                column: "category_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_events",
                table: "calendar_events",
                column: "event_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_custom_tables",
                table: "custom_tables",
                column: "table_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_custom_rows",
                table: "custom_rows",
                column: "row_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_custom_columns",
                table: "custom_columns",
                column: "column_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_custom_cells",
                table: "custom_cells",
                columns: new[] { "row_id", "column_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_calendar_events_category_id",
                table: "calendar_events",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_cells_custom_columns_column_id",
                table: "custom_cells",
                column: "column_id",
                principalTable: "custom_columns",
                principalColumn: "column_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_cells_custom_rows_row_id",
                table: "custom_cells",
                column: "row_id",
                principalTable: "custom_rows",
                principalColumn: "row_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_columns_custom_tables_table_id",
                table: "custom_columns",
                column: "table_id",
                principalTable: "custom_tables",
                principalColumn: "table_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_rows_custom_tables_table_id",
                table: "custom_rows",
                column: "table_id",
                principalTable: "custom_tables",
                principalColumn: "table_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_calendar_events_category_id",
                table: "calendar_events");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_custom_columns_column_id",
                table: "custom_cells");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_cells_custom_rows_row_id",
                table: "custom_cells");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_columns_custom_tables_table_id",
                table: "custom_columns");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_rows_custom_tables_table_id",
                table: "custom_rows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_events",
                table: "calendar_events");

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

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "custom_tables",
                newName: "custom_table");

            migrationBuilder.RenameTable(
                name: "custom_rows",
                newName: "custom_row");

            migrationBuilder.RenameTable(
                name: "custom_columns",
                newName: "custom_column");

            migrationBuilder.RenameTable(
                name: "custom_cells",
                newName: "custom_cell");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "category_name",
                table: "categories",
                newName: "categoryName");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "categories",
                newName: "categoryId");

            migrationBuilder.RenameColumn(
                name: "start_date_time",
                table: "calendar_events",
                newName: "startDateTime");

            migrationBuilder.RenameColumn(
                name: "recurrence_rule",
                table: "calendar_events",
                newName: "recurrenceRule");

            migrationBuilder.RenameColumn(
                name: "recurrence_end",
                table: "calendar_events",
                newName: "recurrenceEnd");

            migrationBuilder.RenameColumn(
                name: "is_all_day",
                table: "calendar_events",
                newName: "isAllDay");

            migrationBuilder.RenameColumn(
                name: "event_title",
                table: "calendar_events",
                newName: "eventTitle");

            migrationBuilder.RenameColumn(
                name: "event_note",
                table: "calendar_events",
                newName: "eventNote");

            migrationBuilder.RenameColumn(
                name: "end_date_time",
                table: "calendar_events",
                newName: "endDateTime");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "calendar_events",
                newName: "categoryId");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "calendar_events",
                newName: "eventId");

            migrationBuilder.RenameIndex(
                name: "ix_calendar_events_category_id",
                table: "calendar_events",
                newName: "IX_calendar_events_categoryId");

            migrationBuilder.RenameColumn(
                name: "table_name",
                table: "custom_table",
                newName: "tableName");

            migrationBuilder.RenameColumn(
                name: "table_id",
                table: "custom_table",
                newName: "tableId");

            migrationBuilder.RenameColumn(
                name: "table_id",
                table: "custom_row",
                newName: "TableId");

            migrationBuilder.RenameColumn(
                name: "row_order",
                table: "custom_row",
                newName: "RowOrder");

            migrationBuilder.RenameColumn(
                name: "row_id",
                table: "custom_row",
                newName: "RowId");

            migrationBuilder.RenameIndex(
                name: "ix_custom_rows_table_id",
                table: "custom_row",
                newName: "IX_custom_row_TableId");

            migrationBuilder.RenameColumn(
                name: "table_id",
                table: "custom_column",
                newName: "TableId");

            migrationBuilder.RenameIndex(
                name: "ix_custom_columns_table_id",
                table: "custom_column",
                newName: "IX_custom_column_TableId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "custom_table",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "custom_row",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                column: "categoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calendar_events",
                table: "calendar_events",
                column: "eventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_table",
                table: "custom_table",
                column: "tableId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_row",
                table: "custom_row",
                column: "RowId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_column",
                table: "custom_column",
                column: "column_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_cell",
                table: "custom_cell",
                columns: new[] { "row_id", "column_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_calendar_events_categoryId",
                table: "calendar_events",
                column: "categoryId",
                principalTable: "categories",
                principalColumn: "categoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_custom_cell_custom_column_column_id",
                table: "custom_cell",
                column: "column_id",
                principalTable: "custom_column",
                principalColumn: "column_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_custom_cell_custom_row_row_id",
                table: "custom_cell",
                column: "row_id",
                principalTable: "custom_row",
                principalColumn: "RowId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_custom_column_custom_table_TableId",
                table: "custom_column",
                column: "TableId",
                principalTable: "custom_table",
                principalColumn: "tableId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_custom_row_custom_table_TableId",
                table: "custom_row",
                column: "TableId",
                principalTable: "custom_table",
                principalColumn: "tableId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
