using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannedIpTrackingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannedIpTracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedIpTracking", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannedIpTracking_Email_Timestamp",
                table: "BannedIpTracking",
                columns: new[] { "Email", "Timestamp" },
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BannedIpTracking_IpAddress_Timestamp",
                table: "BannedIpTracking",
                columns: new[] { "IpAddress", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_BannedIpTracking_Timestamp",
                table: "BannedIpTracking",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedIpTracking");
        }
    }
}
