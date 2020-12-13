using Microsoft.EntityFrameworkCore.Migrations;

namespace digioz.Portal.Web.Data.Migrations
{
    public partial class MigrateUserAndRoleSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ef218a48-e119-4ef5-aac5-334fa3c22d0e", "30886fa5-7839-471b-809d-23549a5a61b8", "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "9c8ed8e4-71a5-442a-b12c-9c239133f9bf", "66019028-8166-460a-97f3-b261bf937a1c", "Moderator", "MODERATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "303c95cf-d698-41ad-bf3e-8fae680bf2f7", "c957b084-3095-4756-b101-cccb34407fdb", "Standard", "STANDARD" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", 0, "4d3acfa8-989c-4daa-af5e-444d8997edf9", "admin@domain.com", true, false, null, "ADMIN@DOMAIN.COM", "ADMIN@DOMAIN.COM", "AQAAAAEAACcQAAAAEJISeglNmBnfpwu6BJXd7jSh9jxeNdX2CXKzSZGUoOIJvRBG4nH5O2NnsGHFbMYcIA==", null, false, "3RCCOF64EJGX4UZTBISAL4PKITHDTWQG", false, "admin@domain.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "ef218a48-e119-4ef5-aac5-334fa3c22d0e", "b4280b6a-0613-4cbd-a9e6-f1701e926e73" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "303c95cf-d698-41ad-bf3e-8fae680bf2f7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9c8ed8e4-71a5-442a-b12c-9c239133f9bf");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "ef218a48-e119-4ef5-aac5-334fa3c22d0e", "b4280b6a-0613-4cbd-a9e6-f1701e926e73" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ef218a48-e119-4ef5-aac5-334fa3c22d0e");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b4280b6a-0613-4cbd-a9e6-f1701e926e73");
        }
    }
}
