using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlippCoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeStylistScheduleIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "StylistScheduleId",
                table: "Bookings",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings",
                column: "StylistScheduleId",
                principalTable: "StylistSchedule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "StylistScheduleId",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_StylistSchedule_StylistScheduleId",
                table: "Bookings",
                column: "StylistScheduleId",
                principalTable: "StylistSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
