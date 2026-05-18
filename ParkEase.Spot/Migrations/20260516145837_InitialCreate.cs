using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkEase.Spot.Migrations
{
    
    public partial class InitialCreate : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "spot");

            migrationBuilder.CreateTable(
                name: "parking_spots",
                schema: "spot",
                columns: table => new
                {
                    spot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lot_id = table.Column<int>(type: "integer", nullable: false),
                    spot_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    floor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    spot_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vehicle_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AVAILABLE"),
                    is_handicapped = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_ev_charging = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    price_per_hour = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_spots", x => x.spot_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parking_spots_lot_id_spot_number",
                schema: "spot",
                table: "parking_spots",
                columns: new[] { "lot_id", "spot_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parking_spots_lot_id_spot_type",
                schema: "spot",
                table: "parking_spots",
                columns: new[] { "lot_id", "spot_type" });

            migrationBuilder.CreateIndex(
                name: "IX_parking_spots_lot_id_status",
                schema: "spot",
                table: "parking_spots",
                columns: new[] { "lot_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_parking_spots_lot_id_vehicle_type",
                schema: "spot",
                table: "parking_spots",
                columns: new[] { "lot_id", "vehicle_type" });
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parking_spots",
                schema: "spot");
        }
    }
}
