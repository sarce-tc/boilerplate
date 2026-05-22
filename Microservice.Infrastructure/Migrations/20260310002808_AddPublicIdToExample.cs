using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToExample : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Examples",
                type: "uuid",
                nullable: true);

            // Assign unique Guid to existing rows (PostgreSQL)
            migrationBuilder.Sql(
                "UPDATE \"Examples\" SET \"PublicId\" = gen_random_uuid() WHERE \"PublicId\" IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Examples",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examples_PublicId",
                table: "Examples",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Examples_PublicId",
                table: "Examples");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Examples");
        }
    }
}
