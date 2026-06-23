using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagmentApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Event_EventId",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Event",
                table: "Event");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Event",
                newName: "events");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "bookings");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_EventId",
                table: "bookings",
                newName: "IX_bookings_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_events",
                table: "events",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bookings",
                table: "bookings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_events_EventId",
                table: "bookings",
                column: "EventId",
                principalTable: "events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_events_EventId",
                table: "bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_events",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bookings",
                table: "bookings");

            migrationBuilder.RenameTable(
                name: "events",
                newName: "Event");

            migrationBuilder.RenameTable(
                name: "bookings",
                newName: "Booking");

            migrationBuilder.RenameIndex(
                name: "IX_bookings_EventId",
                table: "Booking",
                newName: "IX_Booking_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Event",
                table: "Event",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Event_EventId",
                table: "Booking",
                column: "EventId",
                principalTable: "Event",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
