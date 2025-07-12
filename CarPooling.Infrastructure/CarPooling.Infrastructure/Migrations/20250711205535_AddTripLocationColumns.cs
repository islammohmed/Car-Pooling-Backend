using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripLocationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationCity",
                table: "Trips",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "Trips",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "Trips",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SourceCity",
                table: "Trips",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "SourceLatitude",
                table: "Trips",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SourceLongitude",
                table: "Trips",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationCity",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "SourceCity",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "SourceLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "SourceLongitude",
                table: "Trips");
        }
    }
}
