using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlippCoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Service",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Service");
        }
    }
}
