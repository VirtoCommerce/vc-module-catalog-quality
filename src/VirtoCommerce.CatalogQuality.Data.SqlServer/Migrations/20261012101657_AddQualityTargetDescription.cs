using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogQuality.Data.SqlServer.Migrations
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
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
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
