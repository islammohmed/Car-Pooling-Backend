using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelWithHasLoggedIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasLoggedIn",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLoggedIn",
                table: "AspNetUsers");
        }
    }
}
