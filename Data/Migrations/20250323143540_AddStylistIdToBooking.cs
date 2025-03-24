using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlippCoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStylistIdToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_AspNetUsers_CustomerId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Service_ServiceId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_StylistSchedule_StylistScheduleId",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_StylistScheduleId",
                table: "Bookings",
                newName: "IX_Bookings_StylistScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_ServiceId",
                table: "Bookings",
                newName: "IX_Bookings_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_CustomerId",
                table: "Bookings",
                newName: "IX_Bookings_CustomerId");

            migrationBuilder.AddColumn<string>(
                name: "StylistId",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StylistId",
                table: "Bookings",
                column: "StylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_CustomerId",
                table: "Bookings",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_StylistId",
                table: "Bookings",
                column: "StylistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Service_ServiceId",
                table: "Bookings",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings",
                column: "StylistScheduleId",
                principalTable: "StylistSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_CustomerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_StylistId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Service_ServiceId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StylistId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StylistId",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_StylistScheduleId",
                table: "Booking",
                newName: "IX_Booking_StylistScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_ServiceId",
                table: "Booking",
                newName: "IX_Booking_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_CustomerId",
                table: "Booking",
                newName: "IX_Booking_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_AspNetUsers_CustomerId",
                table: "Booking",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Service_ServiceId",
                table: "Booking",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_StylistSchedule_StylistScheduleId",
                table: "Booking",
                column: "StylistScheduleId",
                principalTable: "StylistSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
