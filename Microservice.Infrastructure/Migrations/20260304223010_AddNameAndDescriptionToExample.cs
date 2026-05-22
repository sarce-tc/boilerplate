using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameAndDescriptionToExample : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Examples",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Examples",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Examples");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Examples");
        }
    }
}
