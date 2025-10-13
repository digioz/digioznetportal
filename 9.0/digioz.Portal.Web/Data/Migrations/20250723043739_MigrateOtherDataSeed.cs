﻿using digioz.Portal.Bo;
using digioz.Portal.Dal;
using digioz.Portal.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace digioz.Portal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateOtherDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Config Seed with runtime-generated IDs
            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "IsEncrypted" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "bit" },
                values: new object[,]
                {
                    { Guid.NewGuid().ToString(), "SMTPServer", "mail.domain.com", false },
                    { Guid.NewGuid().ToString(), "SMTPPort", "587", false },
                    { Guid.NewGuid().ToString(), "SMTPUsername", "webmaster@domain.com", false },
                    { Guid.NewGuid().ToString(), "SMTPPassword", "Pass@word1", true },
                    { Guid.NewGuid().ToString(), "SiteURL", "https://localhost:44394/", false },
                    { Guid.NewGuid().ToString(), "SiteName", "DigiOz .NET Portal", false },
                    { Guid.NewGuid().ToString(), "SiteEncryptionKey", "", false },
                    { Guid.NewGuid().ToString(), "WebmasterEmail", "webmaster@domain.com", false },
                    { Guid.NewGuid().ToString(), "PaymentLoginID", "6b74ZBkn5u3", false },
                    { Guid.NewGuid().ToString(), "PaymentTransactionKey", "9M4Tc3s89w3C39cq", false },
                    { Guid.NewGuid().ToString(), "PaymentTestMode", "true", false },
                    { Guid.NewGuid().ToString(), "TwitterHandle", "@digioz", false },
                    { Guid.NewGuid().ToString(), "PaymentTransactionFee", "0", false },
                    { Guid.NewGuid().ToString(), "NumberOfAnnouncements", "2", false },
                    { Guid.NewGuid().ToString(), "ShowContactForm", "false", false },
                    { Guid.NewGuid().ToString(), "VisitorSessionPurgePeriod", "30", false },
                    { Guid.NewGuid().ToString(), "PaypalMode", "sandbox", false },
                    { Guid.NewGuid().ToString(), "PaypalClientId", "AfBQZ3rwN5BKZN6QOJL4zBa1-Uph0KpxxrpMz2ro9RQO_W_CT_1-31GaM-iNo5S0WxIO4Z-LJtW5RInf", false },
                    { Guid.NewGuid().ToString(), "PaypalClientSecret", "EGpl6DrqoaOWVysXEatofIjglg1i1XwHwSIhcw7jZ8duvfgxZAI6SeE8TVmbgHOXxJB7pyKW2O5cOhqj", false },
                    { Guid.NewGuid().ToString(), "PaypalConnectionTimeout", "360000", false },
                    { Guid.NewGuid().ToString(), "EnableCommentsOnAllPages", "true", false },
                    { Guid.NewGuid().ToString(), "TinyMCEApiKey", "[Enter Key]", false }
                });

            // Resolve admin user ID at runtime and insert Menu rows
            migrationBuilder.Sql(@"
DECLARE @adminUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');

INSERT INTO Menu (UserId, Name, Location, Controller, Action, Url, Target, Visible, SortOrder, Timestamp) VALUES
(@adminUserId, 'Home', 'TopMenu', 'Home', 'Index', NULL, NULL, 1, 1, GETDATE()),
(@adminUserId, 'About', 'TopMenu', 'Home', 'About', NULL, NULL, 1, 2, GETDATE()),
(@adminUserId, 'Contact', 'TopMenu', 'Home', 'Contact', NULL, NULL, 1, 3, GETDATE()),
(@adminUserId, 'Forum', 'TopMenu', 'Forum', 'Index', NULL, NULL, 0, 4, GETDATE()),
(@adminUserId, 'Links', 'TopMenu', 'Links', 'Index', NULL, NULL, 1, 5, GETDATE()),
(@adminUserId, 'Chat', 'TopMenu', 'Chat', 'Index', NULL, NULL, 1, 6, GETDATE()),
(@adminUserId, 'Store', 'TopMenu', 'Store', 'Index', NULL, NULL, 0, 7, GETDATE()),
(@adminUserId, 'Twitter', 'TopMenu', 'Twitter', 'Index', NULL, NULL, 0, 8, GETDATE()),
(@adminUserId, 'Home', 'LeftMenu', 'Home', 'Index', NULL, NULL, 1, 9, GETDATE()),
(@adminUserId, 'Pictures', 'LeftMenu', 'Pictures', 'Index', NULL, NULL, 1, 10, GETDATE()),
(@adminUserId, 'Videos', 'LeftMenu', 'Videos', 'Index', NULL, NULL, 1, 11, GETDATE());
");

            // Insert Page rows with admin user resolved at runtime
            migrationBuilder.Sql(@"
DECLARE @adminUserId2 nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');

INSERT INTO Page (UserId, Title, Url, Body, Keywords, Description, Visible, Timestamp) VALUES
(@adminUserId2, 'Home', '/Home/Index', N'<p><span style=""font-size: medium;""><strong>Welcome to DigiOz .NET Portal!</strong></span></p>
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
                        <li>Announcements section let''s you add, edit or remove site wide announcements to the users, which 
                        show up on the Home Index Page.</li>
                        <li>Picture Manager lets you create Picture Galleries, and add or remove pictures from the site.</li>
                        <li>Video Manager allows you to upload and display Videos to your site and manage them.</li>
                        <li>Link Manager allows you to create a links page to do link exchagne with other sites similar to yours.</li>
                        <li>Chat Manager lets you manage the Chat Database Table.</li>
                        </ul>
                        <p><strong><span style=""font-size: medium;"">About DigiOz Multimedia, Inc</span></strong></p>
                        <p><strong><span style=""font-size: medium;""></span></strong>DigiOz Multimedia, Inc is a Chicago, Illinois 
                        USA based Software Development Company which provides web design for personal and business use, CRM, 
                        custom programming for web and PC, design database driven systems for clients, as well as business process 
                        modeling and consulting. We also have an active Open Source Community that provides many IT Systems and Web 
                        Portals as Open Source Products for Everyone to share and enjoy.</p>
                        <p>Visit us at <a href=""http://www.digioz.com"">www.digioz.com</a> for more information, or email us at 
                        <a href=""mailto:support@digioz.com"">support@digioz.com</a>. </p>', NULL, NULL, 1, GETDATE()),
(@adminUserId2, 'Contact', '/Home/Contact', N'<h2>Contact</h2>
                        <h3>Below is our Contact Information:</h3>
                        <address>One Microsoft Way<br /> Redmond, WA 98052-6399<br /> <abbr title=""Phone"">P:</abbr> 425.555.0100</address>
                        <address><strong>Support:</strong> <a href=""mailto:Support@example.com"">Support@example.com</a><br /> <strong>
                        Marketing:</strong> <a href=""mailto:Marketing@example.com"">Marketing@example.com</a></address>', NULL, NULL, 1, GETDATE()),
(@adminUserId2, 'About', '/Home/About', N'<h2>About</h2>
                        <h3>Some information about us:</h3>
                        <p>Use this area to provide additional information.</p>', NULL, NULL, 1, GETDATE());
");

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
            // Remove Config Seed (IDs generated at runtime -> delete by keys)
            migrationBuilder.Sql(@"
DELETE FROM Config WHERE ConfigKey IN (
    'SMTPServer','SMTPPort','SMTPUsername','SMTPPassword','SiteURL','SiteName','SiteEncryptionKey','WebmasterEmail',
    'PaymentLoginID','PaymentTransactionKey','PaymentTestMode','TwitterHandle','PaymentTransactionFee','NumberOfAnnouncements',
    'ShowContactForm','VisitorSessionPurgePeriod','PaypalMode','PaypalClientId','PaypalClientSecret','PaypalConnectionTimeout',
    'EnableCommentsOnAllPages','TinyMCEApiKey'
);
");

            // Remove Menu Seed (resolve admin user and delete seeded items)
            migrationBuilder.Sql(@"
DECLARE @adminUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');
DELETE FROM Menu WHERE UserId = @adminUserId AND (Location = 'TopMenu' AND Name IN ('Home','About','Contact','Forum','Links','Chat','Store','Twitter'));
DELETE FROM Menu WHERE UserId = @adminUserId AND (Location = 'LeftMenu' AND Name IN ('Home','Pictures','Videos'));
");

            // Remove Page Seed (resolve admin user and delete seeded pages)
            migrationBuilder.Sql(@"
DECLARE @adminUserId2 nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');
DELETE FROM Page WHERE UserId = @adminUserId2 AND Url IN ('/Home/Index','/Home/Contact','/Home/About');
");

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
