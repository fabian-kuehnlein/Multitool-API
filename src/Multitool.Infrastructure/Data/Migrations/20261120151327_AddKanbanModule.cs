using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Multitool.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kanban");

            migrationBuilder.CreateTable(
                name: "boards",
                schema: "kanban",
                columns: table => new
                {
                    board_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_boards", x => x.board_id);
                    table.ForeignKey(
                        name: "fk_boards_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "board_columns",
                schema: "kanban",
                columns: table => new
                {
                    column_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    board_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_columns", x => x.column_id);
                    table.ForeignKey(
                        name: "FK_board_columns_boards_board_id",
                        column: x => x.board_id,
                        principalSchema: "kanban",
                        principalTable: "boards",
                        principalColumn: "board_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_members",
                schema: "kanban",
                columns: table => new
                {
                    board_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_members", x => new { x.board_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_board_members_boards_board_id",
                        column: x => x.board_id,
                        principalSchema: "kanban",
                        principalTable: "boards",
                        principalColumn: "board_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_board_members_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cards",
                schema: "kanban",
                columns: table => new
                {
                    card_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    column_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cards", x => x.card_id);
                    table.ForeignKey(
                        name: "FK_cards_board_columns_column_id",
                        column: x => x.column_id,
                        principalSchema: "kanban",
                        principalTable: "board_columns",
                        principalColumn: "column_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cards_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_categories",
                schema: "kanban",
                columns: table => new
                {
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_categories", x => new { x.card_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_card_categories_cards_card_id",
                        column: x => x.card_id,
                        principalSchema: "kanban",
                        principalTable: "cards",
                        principalColumn: "card_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_card_categories_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_board_columns_board_id_position",
                schema: "kanban",
                table: "board_columns",
                columns: new[] { "board_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_board_members_user_id",
                schema: "kanban",
                table: "board_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_boards_owner_id",
                schema: "kanban",
                table: "boards",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_card_categories_category_id",
                schema: "kanban",
                table: "card_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_column_id_position",
                schema: "kanban",
                table: "cards",
                columns: new[] { "column_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_cards_created_by_user_id",
                schema: "kanban",
                table: "cards",
                column: "created_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_members",
                schema: "kanban");

            migrationBuilder.DropTable(
                name: "card_categories",
                schema: "kanban");

            migrationBuilder.DropTable(
                name: "cards",
                schema: "kanban");

            migrationBuilder.DropTable(
                name: "board_columns",
                schema: "kanban");

            migrationBuilder.DropTable(
                name: "boards",
                schema: "kanban");
        }
    }
}
