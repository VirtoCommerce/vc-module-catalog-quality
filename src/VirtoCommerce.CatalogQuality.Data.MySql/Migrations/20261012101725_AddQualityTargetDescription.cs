using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogQuality.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityTargetDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "QualityTarget",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "QualityTarget");
        }
    }
}
