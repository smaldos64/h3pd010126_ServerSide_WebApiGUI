using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GUIWebApi.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories1",
                columns: table => new
                {
                    Category1Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories1", x => x.Category1Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryFiles",
                columns: table => new
                {
                    InventoryFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryFiles", x => x.InventoryFileId);
                });

            migrationBuilder.CreateTable(
                name: "UserFiles",
                columns: table => new
                {
                    UserFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventoryFileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFiles", x => x.UserFileId);
                    table.ForeignKey(
                        name: "FK_UserFiles_InventoryFiles_InventoryFileId",
                        column: x => x.InventoryFileId,
                        principalTable: "InventoryFiles",
                        principalColumn: "InventoryFileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products1",
                columns: table => new
                {
                    Product1Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category1Id = table.Column<int>(type: "int", nullable: false),
                    UserFileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products1", x => x.Product1Id);
                    table.ForeignKey(
                        name: "FK_Products1_Categories1_Category1Id",
                        column: x => x.Category1Id,
                        principalTable: "Categories1",
                        principalColumn: "Category1Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products1_UserFiles_UserFileId",
                        column: x => x.UserFileId,
                        principalTable: "UserFiles",
                        principalColumn: "UserFileId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products1_Category1Id",
                table: "Products1",
                column: "Category1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products1_UserFileId",
                table: "Products1",
                column: "UserFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFiles_InventoryFileId",
                table: "UserFiles",
                column: "InventoryFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products1");

            migrationBuilder.DropTable(
                name: "Categories1");

            migrationBuilder.DropTable(
                name: "UserFiles");

            migrationBuilder.DropTable(
                name: "InventoryFiles");
        }
    }
}
