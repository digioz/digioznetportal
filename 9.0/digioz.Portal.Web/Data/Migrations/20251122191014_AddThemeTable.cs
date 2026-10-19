using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Theme table
            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theme", x => x.Id);
                });

            // Add ThemeId column to Profile table
            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "Profile",
                type: "int",
                nullable: true);

            // Create foreign key relationship
            migrationBuilder.CreateIndex(
                name: "IX_Profile_ThemeId",
                table: "Profile",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_Theme_ThemeId",
                table: "Profile",
                column: "ThemeId",
                principalTable: "Theme",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Insert default theme using raw SQL
            migrationBuilder.Sql(@"
                INSERT INTO Theme (Name, Body, CreateDate, IsDefault)
                VALUES (N'Default', N'', GETDATE(), 1)
            ");

            // Get the path to the CSS overrides folder
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "overrides");

            // Read and insert CSS files if they exist
            if (Directory.Exists(webRootPath))
            {
                var cssFiles = Directory.GetFiles(webRootPath, "*.css");

                foreach (var cssFile in cssFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileNameWithoutExtension(cssFile);
                        string cssContent = File.ReadAllText(cssFile);

                        // Escape single quotes in CSS content for SQL
                        string escapedContent = cssContent.Replace("'", "''");

                        // Use raw SQL to insert theme
                        migrationBuilder.Sql($@"
                            INSERT INTO Theme (Name, Body, CreateDate, IsDefault)
                            VALUES (N'{fileName}', N'{escapedContent}', GETDATE(), 0)
                        ");
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other files
                        Console.WriteLine($"Error reading CSS file {cssFile}: {ex.Message}");
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_Theme_ThemeId",
                table: "Profile");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_Profile_ThemeId",
                table: "Profile");

            // Drop ThemeId column from Profile
            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "Profile");

            // Drop Theme table
            migrationBuilder.DropTable(
                name: "Theme");
        }
    }
}
