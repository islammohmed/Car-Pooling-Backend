using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTimestampsAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "DeliveryRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DeliveryRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "DeliveryRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                table: "DeliveryRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "DeliveryRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "DeliveryRequests");
        }
    }
}
