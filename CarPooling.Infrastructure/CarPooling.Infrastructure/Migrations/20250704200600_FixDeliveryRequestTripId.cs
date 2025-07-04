using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDeliveryRequestTripId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptsDeliveries",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "MaxDeliveryWeight",
                table: "Trips",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripId1",
                table: "DeliveryRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_TripId1",
                table: "DeliveryRequests",
                column: "TripId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequests_Trips_TripId1",
                table: "DeliveryRequests",
                column: "TripId1",
                principalTable: "Trips",
                principalColumn: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequests_Trips_TripId1",
                table: "DeliveryRequests");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequests_TripId1",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "AcceptsDeliveries",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "MaxDeliveryWeight",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TripId1",
                table: "DeliveryRequests");
        }
    }
}
