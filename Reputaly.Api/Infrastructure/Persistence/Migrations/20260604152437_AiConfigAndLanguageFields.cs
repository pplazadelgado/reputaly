using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reputaly.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AiConfigAndLanguageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_LocationId_GoogleReviewId",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "DefaultResponseLanguage",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "es");

            migrationBuilder.AddColumn<string>(
                name: "AiConfig",
                table: "TenantSettings",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<bool>(
                name: "AutoDetectLanguage",
                table: "TenantSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Vertical",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleAccountEmail",
                table: "TenantLocations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ResponseLanguage",
                table: "TenantLocations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LocationId",
                table: "Reviews",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews",
                column: "LocationId",
                principalTable: "TenantLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_LocationId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AiConfig",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "AutoDetectLanguage",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "Vertical",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ResponseLanguage",
                table: "TenantLocations");

            migrationBuilder.RenameColumn(
                name: "DefaultResponseLanguage",
                table: "TenantSettings",
                newName: "AiPersonality");

            migrationBuilder.RenameColumn(
                name: "GoogleTokenExpiresAt",
                table: "TenantLocations",
                newName: "GoogleTokenEspiresAt");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleAccountEmail",
                table: "TenantLocations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LocationId_GoogleReviewId",
                table: "Reviews",
                columns: new[] { "LocationId", "GoogleReviewId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews",
                column: "LocationId",
                principalTable: "TenantLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
