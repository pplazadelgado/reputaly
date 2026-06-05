using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reputaly.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichReviewEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_LocationId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_TenantId",
                table: "Reviews");

            migrationBuilder.AddColumn<DateTime>(
                name: "AiAnalyzedAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoReplied",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DetectedLanguage",
                table: "Reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "DetectedTopics",
                table: "Reviews",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SentimentScore",
                table: "Reviews",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LocationId_GoogleReviewId",
                table: "Reviews",
                columns: new[] { "LocationId", "GoogleReviewId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TenantId_LocationId_Status",
                table: "Reviews",
                columns: new[] { "TenantId", "LocationId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews",
                column: "LocationId",
                principalTable: "TenantLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_LocationId_GoogleReviewId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_TenantId_LocationId_Status",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AiAnalyzedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AutoReplied",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "DetectedLanguage",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "DetectedTopics",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LocationId",
                table: "Reviews",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TenantId",
                table: "Reviews",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TenantLocations_LocationId",
                table: "Reviews",
                column: "LocationId",
                principalTable: "TenantLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
