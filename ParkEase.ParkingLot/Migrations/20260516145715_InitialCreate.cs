using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkEase.ParkingLot.Migrations
{
    
    public partial class InitialCreate : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "parking");

            migrationBuilder.CreateTable(
                name: "parking_lots",
                schema: "parking",
                columns: table => new
                {
                    lot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manager_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    total_spots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    available_spots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    open_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    close_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    approval_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PENDING_APPROVAL"),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by_admin_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_lots", x => x.lot_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parking_lots_city",
                schema: "parking",
                table: "parking_lots",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_parking_lots_manager_id",
                schema: "parking",
                table: "parking_lots",
                column: "manager_id");
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parking_lots",
                schema: "parking");
        }
    }
}
