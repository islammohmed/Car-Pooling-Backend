using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPooling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTripParticipantUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripParticipants_AspNetUsers_UserId1",
                table: "TripParticipants");

            migrationBuilder.DropIndex(
                name: "IX_TripParticipants_UserId1",
                table: "TripParticipants");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TripParticipants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "TripParticipants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripParticipants_UserId1",
                table: "TripParticipants",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TripParticipants_AspNetUsers_UserId1",
                table: "TripParticipants",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
