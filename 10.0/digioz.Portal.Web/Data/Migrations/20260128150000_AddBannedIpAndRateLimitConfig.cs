using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannedIpAndRateLimitConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannedIp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BanExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BanCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttemptedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedIp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannedIp_IpAddress",
                table: "BannedIp",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_BannedIp_BanExpiry",
                table: "BannedIp",
                column: "BanExpiry");

            // Insert Rate Limiting Configuration Settings
            migrationBuilder.Sql(@"
                INSERT INTO [Config] ([Id], [ConfigKey], [ConfigValue], [IsEncrypted])
                VALUES 
                    (NEWID(), 'RateLimit.MaxRequestsPerMinute', '600', 0),
                    (NEWID(), 'RateLimit.MaxRequestsPer10Minutes', '2000', 0),
                    (NEWID(), 'RateLimit.BanDurationMinutes', '60', 0),
                    (NEWID(), 'RateLimit.PermanentBanThreshold', '5', 0),
                    (NEWID(), 'RateLimit.PasswordReset.MaxAttemptsPerIpPerHour', '10', 0),
                    (NEWID(), 'RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour', '3', 0),
                    (NEWID(), 'RateLimit.Login.MaxAttemptsPerIpPerHour', '10', 0),
                    (NEWID(), 'RateLimit.Login.MaxAttemptsPerEmailPerHour', '5', 0),
                    (NEWID(), 'RateLimit.Registration.MaxAttemptsPerIpPerHour', '10', 0),
                    (NEWID(), 'RateLimit.Registration.MaxAttemptsPerEmailPerHour', '5', 0)
            ");

            // Insert Plugin Record for Rate Limiting (Enabled by default)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Plugin] WHERE [Name] = 'Rate Limiting & Bot Protection')
                BEGIN
                    INSERT INTO [Plugin] ([Name], [DLL], [IsEnabled])
                    VALUES ('Rate Limiting & Bot Protection', 'digioz.Portal.Web.Middleware.RateLimitingMiddleware', 0)
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedIp");

            // Remove Rate Limiting Configuration Settings
            migrationBuilder.Sql(@"
                DELETE FROM [Config] 
                WHERE [ConfigKey] LIKE 'RateLimit.%'
            ");

            // Remove Plugin Record
            migrationBuilder.Sql(@"
                DELETE FROM [Plugin] 
                WHERE [Name] = 'Rate Limiting & Bot Protection'
            ");
        }
    }
}
