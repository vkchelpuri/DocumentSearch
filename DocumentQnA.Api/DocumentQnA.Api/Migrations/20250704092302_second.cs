using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentQnA.Api.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "DocumentText");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentText",
                table: "DocumentText",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentText",
                table: "DocumentText");

            migrationBuilder.RenameTable(
                name: "DocumentText",
                newName: "Documents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");
        }
    }
}
