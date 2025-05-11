using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nigar_payment_service.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""Payments""
                ALTER COLUMN ""BookingId""
                TYPE bigint
                USING ""BookingId""::bigint;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""Payments""
                ALTER COLUMN ""BookingId""
                TYPE text
                USING ""BookingId""::text;
            ");
        }
    }
}
