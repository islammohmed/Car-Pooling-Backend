using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeDeliveryTripIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "DeliveryRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CarLicenseImage",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarLicenseImage",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Cars");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "DeliveryRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
