using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reputaly.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropAiPersonalityColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiPersonality",
                table: "TenantSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiPersonality",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
