using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Stories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Chapters",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Authors",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_IsDeleted_DeletedAt",
                table: "Stories",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_IsDeleted_DeletedAt",
                table: "Chapters",
                columns: new[] { "IsDeleted", "DeletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stories_IsDeleted_DeletedAt",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_IsDeleted_DeletedAt",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Authors");
        }
    }
}
