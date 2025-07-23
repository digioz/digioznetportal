using digioz.Portal.Bo;
using digioz.Portal.Dal;
using digioz.Portal.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateOtherDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Config Seed
            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "IsEncrypted" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "bit" },
                values: new object[,]
                {
                    { "139b2ea9-9f7c-46a4-ae2b-5093980c8d55", "SMTPServer", "mail.domain.com", false },
                    { "05f2eb10-9b9b-44ed-984d-be0172027931", "SMTPPort", "587", false },
                    { "010cc9e4-f97c-4eda-8967-0dc236aa05c4", "SMTPUsername", "webmaster@domain.com", false },
                    { "7b35120d-0fb6-4829-9614-2855e48d532d", "SMTPPassword", "Pass@word1", true },
                    { "29d234af-a801-44ec-b9e7-f9406bd886bd", "SiteURL", "https://localhost:44394/", false },
                    { "d532c723-70b9-4320-9f94-c173159772e5", "SiteName", "DigiOz .NET Portal", false },
                    { "a32696b0-d05a-4cdc-b00d-c3415920de26", "SiteEncryptionKey", "", false },
                    { "eed98de0-341a-4aa3-bf45-6e35a8e353bb", "WebmasterEmail", "webmaster@domain.com", false },
                    { "98dc1312-a620-4048-ad0b-f3ae47668c6e", "PaymentLoginID", "6b74ZBkn5u3", false },
                    { "d73f0d79-89d9-435e-a772-3d6ab73b40cf", "PaymentTransactionKey", "9M4Tc3s89w3C39cq", false },
                    { "17faf067-748f-4604-a84a-58871dea9732", "PaymentTestMode", "true", false },
                    { "4c0cbb7a-9505-4730-89e4-77ad7ac479c2", "TwitterHandle", "@digioz", false },
                    { "7c76de85-00a4-426c-a7e5-7f5d2892891f", "PaymentTransactionFee", "0", false },
                    { "ddfbd259-c194-472b-b56b-d400f7ded9d2", "NumberOfAnnouncements", "2", false },
                    { "1e87cf78-4d28-4afa-95e1-159b7e5767b0", "ShowContactForm", "false", false },
                    { "0728371a-6378-4876-a680-7e526ae22803", "VisitorSessionPurgePeriod", "30", false },
                    { "3b679709-b0ce-4e89-ac9e-dec2e30c1b7c", "PaypalMode", "sandbox", false },
                    { "2181f6f8-8ad8-492e-88dc-19967641d084", "PaypalClientId", "AfBQZ3rwN5BKZN6QOJL4zBa1-Uph0KpxxrpMz2ro9RQO_W_CT_1-31GaM-iNo5S0WxIO4Z-LJtW5RInf", false },
                    { "55950eec-2228-43a8-ac87-49a7ab3cb55c", "PaypalClientSecret", "EGpl6DrqoaOWVysXEatofIjglg1i1XwHwSIhcw7jZ8duvfgxZAI6SeE8TVmbgHOXxJB7pyKW2O5cOhqj", false },
                    { "5d7f4e3d-1674-45af-b51d-9f207f9bd057", "PaypalConnectionTimeout", "360000", false },
                    { "a490804e-510f-4350-b500-80ec22636f98", "EnableCommentsOnAllPages", "true", false },
                    { "cfce22eb-349b-4277-b48f-08f90a8200e1", "TinyMCEApiKey", "[Enter Key]", false }
                });

            // Insert Menu Seed with explicit column types
            migrationBuilder.InsertData(
                table: "Menu",
                columns: new[] { "UserId", "Name", "Location", "Controller", "Action", "Url", "Target", "Visible", "SortOrder", "Timestamp" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "bit", "int", "datetime" },
                values: new object[,]
                {
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "TopMenu", "Home", "Index", null, null, true, 1, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "About", "TopMenu", "Home", "About", null, null, true, 2, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Contact", "TopMenu", "Home", "Contact", null, null, true, 3, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Forum", "TopMenu", "Forum", "Index", null, null, false, 4, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Links", "TopMenu", "Links", "Index", null, null, true, 5, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Chat", "TopMenu", "Chat", "Index", null, null, true, 6, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Store", "TopMenu", "Store", "Index", null, null, false, 7, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Twitter", "TopMenu", "Twitter", "Index", null, null, false, 8, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "LeftMenu", "Home", "Index", null, null, true, 9, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Pictures", "LeftMenu", "Pictures", "Index", null, null, true, 10, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Videos", "LeftMenu", "Videos", "Index", null, null, true, 11, DateTime.Now }
                });

            // Insert Page Seed with explicit column types
            migrationBuilder.InsertData(
                table: "Page",
                columns: new[] { "UserId", "Title", "Url", "Body", "Keywords", "Description", "Visible", "Timestamp" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(128)", "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "nvarchar(max)", "bit", "datetime" },
                values: new object[,]
                {
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "/Home/Index", @"<p><span style=''font-size: medium;''><strong>Welcome to DigiOz .NET Portal!</strong></span></p>
                        <p>DigiOz .NET Portal is a web based portal system written in ASP.NET Razor Pages written with .NET 9.0 and &nbsp; C# &nbsp; which uses a 
                        Microsoft SQL Database to allows webmasters to setup and customize an instant website for either business 
                        or personal use.</p>
                        <p>Some Features included in this Portal System include:</p>
                        <ul>
                        <li>An Administrative Dashboard, where the Webmaster can Manage the Site and related Features.</li>
                        <li>A Page Manager, to allow Admins to Create new Pages, Edit existing Pages or Delete Them.</li>
                        <li>A Database Driven Configuration System to fine tune the Portal System</li>
                        <li>Some Database Utilities to help Manage the Site Database</li>
                        <li>File Manager, which allows you to add or remove files to your site.</li>
                        <li>Module Manager, allow you to install new Plugins to the Portal.</li>
                        <li>Forum Manager allows you to Manage Forum Posts, Threads, and Users.</li>
                        <li>Poll Manager lets you create new polls to display on the site.</li>
                        <li>The Statistics section lets you see site related statistics such as the number of visitors, number of 
                        page Views, etc.</li>
                        <li>Menu Manager lets you add new Menu links both to internal pages and external sites.</li>
                        <li>User Manager lets you manage the registered users of the site.</li>
                        <li>Announcements section let''''s you add, edit or remove site wide announcements to the users, which 
                        show up on the Home Index Page.</li>
                        <li>Picture Manager lets you create Picture Galleries, and add or remove pictures from the site.</li>
                        <li>Video Manager allows you to upload and display Videos to your site and manage them.</li>
                        <li>Link Manager allows you to create a links page to do link exchagne with other sites similar to yours.</li>
                        <li>Chat Manager lets you manage the Chat Database Table.</li>
                        </ul>
                        <p><strong><span style=''font-size: medium;''>About DigiOz Multimedia, Inc</span></strong></p>
                        <p><strong><span style=''font-size: medium;''></span></strong>DigiOz Multimedia, Inc is a Chicago, Illinois 
                        USA based Software Development Company which provides web design for personal and business use, CRM, 
                        custom programming for web and PC, design database driven systems for clients, as well as business process 
                        modeling and consulting. We also have an active Open Source Community that provides many IT Systems and Web 
                        Portals as Open Source Products for Everyone to share and enjoy.</p>
                        <p>Visit us at <a href=''http://www.digioz.com''>www.digioz.com</a> for more information, or email us at 
                        <a href=''mailto:support@digioz.com''>support@digioz.com</a>. </p>", null, null, true, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Contact", "/Home/Contact", @"<h2>Contact</h2>
                        <h3>Below is our Contact Information:</h3>
                        <address>One Microsoft Way<br /> Redmond, WA 98052-6399<br /> <abbr title=''Phone''>P:</abbr> 425.555.0100</address>
                        <address><strong>Support:</strong> <a href=''mailto:Support@example.com''>Support@example.com</a><br /> <strong>
                        Marketing:</strong> <a href=''mailto:Marketing@example.com''>Marketing@example.com</a></address>", null, null, true, DateTime.Now },
                    { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "About", "/Home/About", @"<h2>About</h2>
                        <h3>Some information about us:</h3>
                        <p>Use this area to provide additional information.</p>", null, null, true, DateTime.Now }
                });

            // Insert Plugin Seed with explicit column types
            migrationBuilder.InsertData(
                table: "Plugin",
                columns: new[] { "Name", "Dll", "IsEnabled" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(128)", "bit" },
                values: new object[,]
                {
                    { "Chat", null, true },
                    { "Store", null, true },
                    { "Twitter", null, false },
                    { "WhoIsOnline", null, true },
                    { "SlideShow", null, false },
                    { "Comments", null, false },
                    { "RSSFeed", null, false },
                    { "LatestPictures", null, false },
                    { "LatestVideos", null, false }
                });

            // Insert Zone Seed with explicit column types
            migrationBuilder.InsertData(
                table: "Zone",
                columns: new[] { "Name", "ZoneType" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(128)" },
                values: new object[,]
                {
                    { "Top", "Module" },
                    { "TopMenu", "Menu" },
                    { "Left", "Module" },
                    { "LeftMenu", "Menu" },
                    { "BodyTop", "Module" },
                    { "BodyBottom", "Module" },
                    { "Bottom", "Module" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Config Seed
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "139b2ea9-9f7c-46a4-ae2b-5093980c8d55");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "05f2eb10-9b9b-44ed-984d-be0172027931");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "010cc9e4-f97c-4eda-8967-0dc236aa05c4");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "7b35120d-0fb6-4829-9614-2855e48d532d");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "29d234af-a801-44ec-b9e7-f9406bd886bd");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "d532c723-70b9-4320-9f94-c173159772e5");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "a32696b0-d05a-4cdc-b00d-c3415920de26");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "eed98de0-341a-4aa3-bf45-6e35a8e353bb");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "98dc1312-a620-4048-ad0b-f3ae47668c6e");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "d73f0d79-89d9-435e-a772-3d6ab73b40cf");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "17faf067-748f-4604-a84a-58871dea9732");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "4c0cbb7a-9505-4730-89e4-77ad7ac479c2");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "7c76de85-00a4-426c-a7e5-7f5d2892891f");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "ddfbd259-c194-472b-b56b-d400f7ded9d2");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "1e87cf78-4d28-4afa-95e1-159b7e5767b0");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "0728371a-6378-4876-a680-7e526ae22803");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "3b679709-b0ce-4e89-ac9e-dec2e30c1b7c");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "2181f6f8-8ad8-492e-88dc-19967641d084");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "55950eec-2228-43a8-ac87-49a7ab3cb55c");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "5d7f4e3d-1674-45af-b51d-9f207f9bd057");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "a490804e-510f-4350-b500-80ec22636f98");
            migrationBuilder.DeleteData(table: "Config", keyColumn: "Id", keyValue: "cfce22eb-349b-4277-b48f-08f90a8200e1");

            // Remove Menu Seed
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "About", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Contact", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Forum", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Links", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Chat", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Store", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Twitter", "TopMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "LeftMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Pictures", "LeftMenu" });
            migrationBuilder.DeleteData(table: "Menu", keyColumns: new[] { "UserId", "Name", "Location" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Videos", "LeftMenu" });

            // Remove Page Seed
            migrationBuilder.DeleteData(table: "Page", keyColumns: new[] { "UserId", "Title", "Url" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Home", "/Home/Index" });
            migrationBuilder.DeleteData(table: "Page", keyColumns: new[] { "UserId", "Title", "Url" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "Contact", "/Home/Contact" });
            migrationBuilder.DeleteData(table: "Page", keyColumns: new[] { "UserId", "Title", "Url" }, keyValues: new object[] { "b4280b6a-0613-4cbd-a9e6-f1701e926e73", "About", "/Home/About" });

            // Remove Plugin Seed
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Chat");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Store");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Twitter");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "WhoIsOnline");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "SlideShow");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Comments");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "RSSFeed");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "LatestPictures");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "LatestVideos");

            // Remove Zone Seed
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "Top");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "TopMenu");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "Left");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "LeftMenu");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "BodyTop");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "BodyBottom");
            migrationBuilder.DeleteData(table: "Zone", keyColumn: "Name", keyValue: "Bottom");
        }
    }
}
