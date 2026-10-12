using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogQuality.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "QualityData",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "QualityData",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TargetId",
                table: "QualityData",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QualityTarget",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityTarget", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityTarget");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "QualityData");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "QualityData",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "QualityData",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }
    }
}
