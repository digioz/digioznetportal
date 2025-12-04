using digioz.Portal.Bo;
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
            // Insert Profile record for admin user (lookup UserId at runtime)
            migrationBuilder.Sql(@"
DECLARE @adminUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');

INSERT INTO Profile (UserId, DisplayName, FirstName, MiddleName, LastName, Email, Birthday, BirthdayVisible, Address, Address2, City, State, Zip, Country, Signature, Avatar)
VALUES (@adminUserId, 'Administrator', 'Admin', NULL, 'User', 'admin@domain.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

DECLARE @systemUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'system@domain.com');

INSERT INTO Profile (UserId, DisplayName, FirstName, MiddleName, LastName, Email, Birthday, BirthdayVisible, Address, Address2, City, State, Zip, Country, Signature, Avatar)
VALUES (@systemUserId, 'System', 'System', NULL, 'User', 'system@domain.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
");

            // Insert Config Seed with runtime-generated IDs
            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "IsEncrypted" },
                columnTypes: new[] { "nvarchar(128)", "nvarchar(max)", "nvarchar(max)", "bit" },
                values: new object[,]
                {
                    // Email General Configuration
                    { Guid.NewGuid().ToString(), "Email:ProviderType", "SMTP", false },
                    { Guid.NewGuid().ToString(), "Email:FromEmail", "noreply@domain.com", false },
                    { Guid.NewGuid().ToString(), "Email:FromName", "DigiOz .NET Portal", false },
                    { Guid.NewGuid().ToString(), "Email:IsEnabled", "true", false },
                    { Guid.NewGuid().ToString(), "Email:TimeoutSeconds", "30", false },
                    
                    // SMTP Configuration
                    { Guid.NewGuid().ToString(), "Email:Smtp:Host", "mail.domain.com", false },
                    { Guid.NewGuid().ToString(), "Email:Smtp:Port", "587", false },
                    { Guid.NewGuid().ToString(), "Email:Smtp:Username", "webmaster@domain.com", false },
                    { Guid.NewGuid().ToString(), "Email:Smtp:Password", "[Enter Password]", true },
                    { Guid.NewGuid().ToString(), "Email:Smtp:EnableSsl", "true", false },
                    { Guid.NewGuid().ToString(), "Email:Smtp:UseDefaultCredentials", "false", false },
                    
                    // SendGrid Configuration
                    { Guid.NewGuid().ToString(), "Email:SendGrid:ApiKey", "[Enter SendGrid API Key]", true },
                    { Guid.NewGuid().ToString(), "Email:SendGrid:SandboxMode", "false", false },
                    { Guid.NewGuid().ToString(), "Email:SendGrid:EnableClickTracking", "true", false },
                    { Guid.NewGuid().ToString(), "Email:SendGrid:EnableOpenTracking", "true", false },
                    { Guid.NewGuid().ToString(), "Email:SendGrid:TemplateId", "", false },
                    
                    // Mailgun Configuration
                    { Guid.NewGuid().ToString(), "Email:Mailgun:ApiKey", "[Enter Mailgun API Key]", true },
                    { Guid.NewGuid().ToString(), "Email:Mailgun:Domain", "[Enter Mailgun Domain]", false },
                    { Guid.NewGuid().ToString(), "Email:Mailgun:ApiBaseUrl", "https://api.mailgun.net/v3", false },
                    { Guid.NewGuid().ToString(), "Email:Mailgun:EnableTracking", "true", false },
                    { Guid.NewGuid().ToString(), "Email:Mailgun:EnableDkim", "true", false },
                    
                    // Azure Communication Services Email Configuration
                    { Guid.NewGuid().ToString(), "Email:Azure:ConnectionString", "[Enter Azure Communication Services Connection String]", true },
                    { Guid.NewGuid().ToString(), "Email:Azure:EnableTracking", "true", false },
                    
                    // Site Configuration
                    { Guid.NewGuid().ToString(), "SiteURL", "https://localhost:5048/", false },
                    { Guid.NewGuid().ToString(), "SiteName", "DigiOz .NET Portal", false },
                    { Guid.NewGuid().ToString(), "WebmasterEmail", "webmaster@domain.com", false },
                    
                    // Payment Configuration
                    { Guid.NewGuid().ToString(), "PaymentLoginID", "[Enter ID]", false },
                    { Guid.NewGuid().ToString(), "PaymentTransactionKey", "[Enter Key]", false },
                    { Guid.NewGuid().ToString(), "PaymentTestMode", "true", false },
                    { Guid.NewGuid().ToString(), "PaymentTransactionFee", "0", false },
                    
                    // PayPal Configuration
                    { Guid.NewGuid().ToString(), "PaypalMode", "sandbox", false },
                    { Guid.NewGuid().ToString(), "PaypalClientId", "[Enter ID]", false },
                    { Guid.NewGuid().ToString(), "PaypalClientSecret", "[Enter Key]", false },
                    { Guid.NewGuid().ToString(), "PaypalConnectionTimeout", "360000", false },
                    
                    // Site Features Configuration
                    { Guid.NewGuid().ToString(), "NumberOfAnnouncements", "2", false },
                    { Guid.NewGuid().ToString(), "ShowContactForm", "false", false },
                    { Guid.NewGuid().ToString(), "VisitorSessionPurgePeriod", "30", false },
                    { Guid.NewGuid().ToString(), "EnableCommentsOnAllPages", "true", false },
                    
                    // Third-Party Services
                    { Guid.NewGuid().ToString(), "TinyMCEApiKey", "[Enter Key]", false },
                    { Guid.NewGuid().ToString(), "RecaptchaEnabled", "false", false },
                    { Guid.NewGuid().ToString(), "RecaptchaPublicKey", "[Enter Key]", false },
                    { Guid.NewGuid().ToString(), "RecaptchaPrivateKey", "[Enter Key]", false }
                });

            // Resolve admin user ID at runtime and insert Menu rows
            migrationBuilder.Sql(@"
DECLARE @adminUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');

INSERT INTO Menu (UserId, Name, Location, Controller, Action, Url, Target, Visible, SortOrder, Timestamp) VALUES
(@adminUserId, 'Home', 'TopMenu', 'Index', '', NULL, NULL, 1, 1, GETDATE()),
(@adminUserId, 'About', 'TopMenu', 'Home', 'About', NULL, NULL, 1, 2, GETDATE()),
(@adminUserId, 'Contact', 'TopMenu', 'Home', 'Contact', NULL, NULL, 1, 3, GETDATE()),
(@adminUserId, 'Forum', 'TopMenu', 'Forum', 'Index', NULL, NULL, 0, 4, GETDATE()),
(@adminUserId, 'Links', 'TopMenu', 'Links', 'Index', NULL, NULL, 1, 5, GETDATE()),
(@adminUserId, 'Chat', 'TopMenu', 'Chat', 'Index', NULL, NULL, 1, 6, GETDATE()),
(@adminUserId, 'Store', 'TopMenu', 'Store', 'Index', NULL, NULL, 0, 7, GETDATE()),
(@adminUserId, 'Home', 'LeftMenu', 'Index', '', NULL, NULL, 1, 9, GETDATE()),
(@adminUserId, 'Pictures', 'LeftMenu', 'Pictures', 'Index', NULL, NULL, 1, 10, GETDATE()),
(@adminUserId, 'Videos', 'LeftMenu', 'Videos', 'Index', NULL, NULL, 1, 11, GETDATE()),
(@adminUserId, 'Members', 'LeftMenu', 'Profile', 'Index', NULL, NULL, 1, 12, GETDATE()),
(@adminUserId, 'Comments', 'LeftMenu', 'Comments', 'Index', NULL, NULL, 1, 13, GETDATE());
");

            // Insert Page rows with admin user resolved at runtime
            migrationBuilder.Sql(@"
DECLARE @adminUserId2 nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');

INSERT INTO Page (UserId, Title, Url, Body, Keywords, Description, Visible, Timestamp) VALUES
(@adminUserId2, 'Home', '/Index', N'<p><span style=""font-size: medium;""><strong>Welcome to DigiOz .NET Portal!</strong></span></p>
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
                    { "Store", null, false },
                    { "WhoIsOnline", null, true },
                    { "SlideShow", null, false },
                    { "Comments", null, false },
                    { "RSSFeed", null, false },
                    { "LatestPictures", null, false },
                    { "LatestVideos", null, false },
                    { "Polls", null, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Profile record
            migrationBuilder.Sql(@"
DELETE FROM Profile
WHERE UserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');
DELETE FROM Profile
WHERE UserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'system@domain.com');
");

            // Remove Config Seed (IDs generated at runtime -> delete by keys)
            migrationBuilder.Sql(@"
DELETE FROM Config WHERE ConfigKey IN (
    -- Email General Configuration
    'Email:ProviderType','Email:FromEmail','Email:FromName','Email:IsEnabled','Email:TimeoutSeconds',
    -- SMTP Configuration
    'Email:Smtp:Host','Email:Smtp:Port','Email:Smtp:Username','Email:Smtp:Password','Email:Smtp:EnableSsl','Email:Smtp:UseDefaultCredentials',
    -- SendGrid Configuration
    'Email:SendGrid:ApiKey','Email:SendGrid:SandboxMode','Email:SendGrid:EnableClickTracking','Email:SendGrid:EnableOpenTracking','Email:SendGrid:TemplateId',
    -- Mailgun Configuration
    'Email:Mailgun:ApiKey','Email:Mailgun:Domain','Email:Mailgun:ApiBaseUrl','Email:Mailgun:EnableTracking','Email:Mailgun:EnableDkim',
    -- Azure Email Configuration
    'Email:Azure:ConnectionString','Email:Azure:EnableTracking',
    -- Site Configuration
    'SiteURL','SiteName','SiteEncryptionKey','WebmasterEmail',
    -- Payment Configuration
    'PaymentLoginID','PaymentTransactionKey','PaymentTestMode','PaymentTransactionFee',
    -- PayPal Configuration
    'PaypalMode','PaypalClientId','PaypalClientSecret','PaypalConnectionTimeout',
    -- Site Features
    'NumberOfAnnouncements','ShowContactForm','VisitorSessionPurgePeriod','EnableCommentsOnAllPages',
    -- Third-Party Services
    'TinyMCEApiKey','RecaptchaEnabled','RecaptchaPublicKey','RecaptchaPrivateKey'
);
");

            // Remove Menu Seed (resolve admin user and delete seeded items)
            migrationBuilder.Sql(@"
DECLARE @adminUserId nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');
DELETE FROM Menu WHERE UserId = @adminUserId AND (Location = 'TopMenu' AND Name IN ('Home','About','Contact','Forum','Links','Chat','Store'));
DELETE FROM Menu WHERE UserId = @adminUserId AND (Location = 'LeftMenu' AND Name IN ('Home','Pictures','Videos'));
");

            // Remove Page Seed (resolve admin user and delete seeded pages)
            migrationBuilder.Sql(@"
DECLARE @adminUserId2 nvarchar(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'admin@domain.com');
DELETE FROM Page WHERE UserId = @adminUserId2 AND Url IN ('/Index','/Home/Contact','/Home/About');
");

            // Remove Plugin Seed
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Chat");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Store");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "WhoIsOnline");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "SlideShow");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Comments");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "RSSFeed");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "LatestPictures");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "LatestVideos");
            migrationBuilder.DeleteData(table: "Plugin", keyColumn: "Name", keyValue: "Polls");
        }
    }
}
