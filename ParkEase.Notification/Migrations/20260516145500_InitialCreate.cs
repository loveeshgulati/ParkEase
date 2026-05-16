using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkEase.Notification.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipient_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    related_id = table.Column<int>(type: "integer", nullable: true),
                    related_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "APP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_id",
                schema: "notification",
                table: "notifications",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_id_is_read",
                schema: "notification",
                table: "notifications",
                columns: new[] { "recipient_id", "is_read" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification");
        }
    }
}
