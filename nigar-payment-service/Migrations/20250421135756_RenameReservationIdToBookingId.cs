using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nigar_payment_service.Migrations
{
    /// <inheritdoc />
    public partial class RenameReservationIdToBookingId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "Payments",
                newName: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "Payments",
                newName: "ReservationId");
        }
    }
}
