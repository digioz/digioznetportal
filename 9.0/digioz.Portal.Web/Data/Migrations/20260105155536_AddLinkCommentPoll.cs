using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkCommentPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Link",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "Comment",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Comment",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "Poll",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Poll",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "IsEncrypted" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "bit" },
                values: new object[] { Guid.NewGuid().ToString(), "Comment:RequireApproval", "false", false });

            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "IsEncrypted" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "bit" },
                values: new object[] { Guid.NewGuid().ToString(), "Comment:RequireApprovalMinValue", "5", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Link");

            migrationBuilder.DropColumn(
                name: "Visible",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Visible",
                table: "Poll");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Poll");

            migrationBuilder.DeleteData(
                table: "Config",
                keyColumn: "ConfigKey",
                keyValue: "Comment:RequireApproval");

            migrationBuilder.DeleteData(
                table: "Config",
                keyColumn: "ConfigKey",
                keyValue: "Comment:RequireApprovalMinValue");
        }
    }
}
