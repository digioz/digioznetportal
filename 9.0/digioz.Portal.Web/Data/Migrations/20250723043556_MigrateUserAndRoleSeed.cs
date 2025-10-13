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

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { userId, 0, userConcurrencyStamp, "admin@domain.com", true, false, null, "ADMIN@DOMAIN.COM", "ADMIN@DOMAIN.COM", passwordHash, null, false, securityStamp, false, "admin@domain.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { adminRoleId, userId });
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

            // Remove roles by Name (IDs were generated at runtime in Up)
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Name IN ('Administrator','Moderator','Standard');");

            // Remove the admin user by username/email
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE UserName = 'admin@domain.com';");
        }
    }
}
