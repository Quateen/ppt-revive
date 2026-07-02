using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPTRevive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GoogleLoginFieldsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleAud",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleAzp",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleExp",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleIat",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleIss",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GooglePicture",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleSub",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SocialApp",
                table: "User",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleAud",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleAzp",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleExp",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleIat",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleIss",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GooglePicture",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleSub",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SocialApp",
                table: "User");
        }
    }
}
