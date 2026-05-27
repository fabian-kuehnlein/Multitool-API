using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multitool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnIdAutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ColumnId",
                table: "custom_column",
                newName: "column_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "column_id",
                table: "custom_column",
                newName: "ColumnId");
        }
    }
}
