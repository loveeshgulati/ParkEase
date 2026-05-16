using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkEase.Booking.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    lot_id = table.Column<int>(type: "integer", nullable: false),
                    spot_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vehicle_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    booking_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "RESERVED"),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    check_in_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    check_out_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_amount = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.booking_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_lot_id",
                table: "bookings",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_spot_id",
                table: "bookings",
                column: "spot_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_spot_id_status",
                table: "bookings",
                columns: new[] { "spot_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_id",
                table: "bookings",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");
        }
    }
}
