using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentQnA.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorEmbeddingToDocumentText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VectorEmbeddingJson",
                table: "DocumentText",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VectorEmbeddingJson",
                table: "DocumentText");
        }
    }
}
