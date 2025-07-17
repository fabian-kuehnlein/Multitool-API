using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultitoolApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRowPositionToCustomRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RowOrder",
                table: "custom_row",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowOrder",
                table: "custom_row");
        }
    }
}
