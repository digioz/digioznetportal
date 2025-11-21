using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Microsoft.AspNetCore.Identity;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateUserAndRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Generate GUIDs at runtime
            var adminRoleId = Guid.NewGuid().ToString();
            var moderatorRoleId = Guid.NewGuid().ToString();
            var standardRoleId = Guid.NewGuid().ToString();

            var adminRoleConcurrency = Guid.NewGuid().ToString();
            var moderatorRoleConcurrency = Guid.NewGuid().ToString();
            var standardRoleConcurrency = Guid.NewGuid().ToString();

            var userId = Guid.NewGuid().ToString();
            var userConcurrencyStamp = Guid.NewGuid().ToString();
            var securityStamp = Guid.NewGuid().ToString();

            var systemUserId = Guid.NewGuid().ToString();
            var systemUserConcurrencyStamp = Guid.NewGuid().ToString();
            var systemSecurityStamp = Guid.NewGuid().ToString();

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { adminRoleId, adminRoleConcurrency, "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { moderatorRoleId, moderatorRoleConcurrency, "Moderator", "MODERATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { standardRoleId, standardRoleConcurrency, "Standard", "STANDARD" });

            // Generate password hash for Pass@word1 at migration time
            var hasher = new PasswordHasher<IdentityUser>();
            var tempUser = new IdentityUser
            {
                Id = userId,
                UserName = "admin@domain.com",
                NormalizedUserName = "ADMIN@DOMAIN.COM",
                Email = "admin@domain.com",
                NormalizedEmail = "ADMIN@DOMAIN.COM",
                SecurityStamp = securityStamp,
                ConcurrencyStamp = userConcurrencyStamp
            };
            var passwordHash = hasher.HashPassword(tempUser, "Pass@word1");

            // Generate password hash for System user with random password
            var systemTempUser = new IdentityUser
            {
                Id = systemUserId,
                UserName = "system@domain.com",
                NormalizedUserName = "SYSTEM",
                Email = "system@domain.com",
                NormalizedEmail = "SYSTEM@DOMAIN.COM",
                SecurityStamp = systemSecurityStamp,
                ConcurrencyStamp = systemUserConcurrencyStamp
            };
            var systemPasswordHash = hasher.HashPassword(systemTempUser, Guid.NewGuid().ToString());

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { userId, 0, userConcurrencyStamp, "admin@domain.com", true, false, null, "ADMIN@DOMAIN.COM", "ADMIN@DOMAIN.COM", passwordHash, null, false, securityStamp, false, "admin@domain.com" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { systemUserId, 0, systemUserConcurrencyStamp, "system@domain.com", true, false, null, "SYSTEM@DOMAIN.COM", "SYSTEM@DOMAIN.COM", systemPasswordHash, null, false, systemSecurityStamp, false, "system@domain.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { adminRoleId, userId });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { standardRoleId, systemUserId });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove User-Role mapping
            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles
                WHERE UserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com')
                  AND RoleId = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Administrator');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles
                WHERE UserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'system@domain.com')
                  AND RoleId = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Standard');
            ");

            // Remove roles by Name (IDs were generated at runtime in Up)
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Name IN ('Administrator','Moderator','Standard');");

            // Remove the admin user by username/email
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE UserName = 'admin@domain.com';");

            // Remove the System user
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE UserName = 'system@domain.com';");
        }
    }
}
