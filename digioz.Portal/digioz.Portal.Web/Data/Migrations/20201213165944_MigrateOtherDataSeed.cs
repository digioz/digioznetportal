using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using System.Collections.Generic;
using System;

namespace digioz.Portal.Web.Data.Migrations
{
    public partial class MigrateOtherDataSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var context = new digiozPortalContext();

            // Insert Config Seed 
            var configs = new List<Config>();

            configs.Add(new Config() { Id = "139b2ea9-9f7c-46a4-ae2b-5093980c8d55", ConfigKey = "SMTPServer", ConfigValue = "mail.domain.com", IsEncrypted = false });
            configs.Add(new Config() { Id = "05f2eb10-9b9b-44ed-984d-be0172027931", ConfigKey = "SMTPPort", ConfigValue = "587", IsEncrypted = false });
            configs.Add(new Config() { Id = "010cc9e4-f97c-4eda-8967-0dc236aa05c4", ConfigKey = "SMTPUsername", ConfigValue = "webmaster@domain.com", IsEncrypted = false });
            configs.Add(new Config() { Id = "7b35120d-0fb6-4829-9614-2855e48d532d", ConfigKey = "SMTPPassword", ConfigValue = "ohuiChQV5zxEwpmisiiWcIvlEVBH/zaroX1Rd9SD6zU=", IsEncrypted = true });
            configs.Add(new Config() { Id = "29d234af-a801-44ec-b9e7-f9406bd886bd", ConfigKey = "SiteURL", ConfigValue = "http://localhost:57443", IsEncrypted = false });
            configs.Add(new Config() { Id = "d532c723-70b9-4320-9f94-c173159772e5", ConfigKey = "SiteName", ConfigValue = "DigiOz .NET Portal", IsEncrypted = false });
            configs.Add(new Config() { Id = "a32696b0-d05a-4cdc-b00d-c3415920de26", ConfigKey = "SiteEncryptionKey", ConfigValue = "BlAMwXxp7oMxGtWzIZYe", IsEncrypted = false });
            configs.Add(new Config() { Id = "eed98de0-341a-4aa3-bf45-6e35a8e353bb", ConfigKey = "WebmasterEmail", ConfigValue = "webmaster@domain.com", IsEncrypted = false });
            configs.Add(new Config() { Id = "98dc1312-a620-4048-ad0b-f3ae47668c6e", ConfigKey = "PaymentLoginID", ConfigValue = "6b74ZBkn5u3", IsEncrypted = false });
            configs.Add(new Config() { Id = "d73f0d79-89d9-435e-a772-3d6ab73b40cf", ConfigKey = "PaymentTransactionKey", ConfigValue = "9M4Tc3s89w3C39cq", IsEncrypted = false });
            configs.Add(new Config() { Id = "17faf067-748f-4604-a84a-58871dea9732", ConfigKey = "PaymentTestMode", ConfigValue = "true", IsEncrypted = false });
            configs.Add(new Config() { Id = "4c0cbb7a-9505-4730-89e4-77ad7ac479c2", ConfigKey = "TwitterHandle", ConfigValue = "@digioz", IsEncrypted = false });
            configs.Add(new Config() { Id = "7c76de85-00a4-426c-a7e5-7f5d2892891f", ConfigKey = "PaymentTransactionFee", ConfigValue = "0", IsEncrypted = false });
            configs.Add(new Config() { Id = "ddfbd259-c194-472b-b56b-d400f7ded9d2", ConfigKey = "NumberOfAnnouncements", ConfigValue = "2", IsEncrypted = false });
            configs.Add(new Config() { Id = "1e87cf78-4d28-4afa-95e1-159b7e5767b0", ConfigKey = "ShowContactForm", ConfigValue = "false", IsEncrypted = false });
            configs.Add(new Config() { Id = "0728371a-6378-4876-a680-7e526ae22803", ConfigKey = "VisitorSessionPurgePeriod", ConfigValue = "30", IsEncrypted = false });
            configs.Add(new Config() { Id = "3b679709-b0ce-4e89-ac9e-dec2e30c1b7c", ConfigKey = "PaypalMode", ConfigValue = "sandbox", IsEncrypted = false });
            configs.Add(new Config() { Id = "2181f6f8-8ad8-492e-88dc-19967641d084", ConfigKey = "PaypalClientId", ConfigValue = "AfBQZ3rwN5BKZN6QOJL4zBa1-Uph0KpxxrpMz2ro9RQO_W_CT_1-31GaM-iNo5S0WxIO4Z-LJtW5RInf", IsEncrypted = false });
            configs.Add(new Config() { Id = "55950eec-2228-43a8-ac87-49a7ab3cb55c", ConfigKey = "PaypalClientSecret", ConfigValue = "EGpl6DrqoaOWVysXEatofIjglg1i1XwHwSIhcw7jZ8duvfgxZAI6SeE8TVmbgHOXxJB7pyKW2O5cOhqj", IsEncrypted = false });
            configs.Add(new Config() { Id = "5d7f4e3d-1674-45af-b51d-9f207f9bd057", ConfigKey = "PaypalConnectionTimeout", ConfigValue = "360000", IsEncrypted = false });
            configs.Add(new Config() { Id = "a490804e-510f-4350-b500-80ec22636f98", ConfigKey = "EnableCommentsOnAllPages", ConfigValue = "true", IsEncrypted = false });
            configs.Add(new Config() { Id = "cfce22eb-349b-4277-b48f-08f90a8200e1", ConfigKey = "TinyMCEApiKey", ConfigValue = "[Enter Key]", IsEncrypted = false });

            context.Configs.AddRange(configs);
            context.SaveChanges();

            // Insert Menu Seed
            var menus = new List<Menu>();

            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Home", Location = "TopMenu", Controller = "Home", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 1, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "About", Location = "TopMenu", Controller = "Home", Action = "About", Url = null, Target = null, Visible = true, SortOrder = 2, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Contact", Location = "TopMenu", Controller = "Home", Action = "Contact", Url = null, Target = null, Visible = true, SortOrder = 3, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Forum", Location = "TopMenu", Controller = "Forum", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 4, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Links", Location = "TopMenu", Controller = "Links", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 5, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Chat", Location = "TopMenu", Controller = "Chat", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 6, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Store", Location = "TopMenu", Controller = "Store", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 7, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Twitter", Location = "TopMenu", Controller = "Twitter", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 8, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Home", Location = "LeftMenu", Controller = "Home", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 9, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Pictures", Location = "LeftMenu", Controller = "Pictures", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 10, Timestamp = DateTime.Now });
            menus.Add(new Menu() { UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73", Name = "Videos", Location = "LeftMenu", Controller = "Videos", Action = "Index", Url = null, Target = null, Visible = true, SortOrder = 11, Timestamp = DateTime.Now });

            context.Menus.AddRange(menus);
            context.SaveChanges();

            // Insert Page Seed
            var pages = new List<Page>();

            pages.Add(new Page()
            {
                UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73",
                Title = "Home",
                Url = "/Home/Index",
                Body = @"<p><span style=''font-size: medium;''><strong>Welcome to DigiOz .NET Portal!</strong></span></p>
                        <p>DigiOz .NET Portal is a web based portal system written in ASP.NET MVC Core 5.0 and&nbsp;C#&nbsp;which uses a 
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
                        <a href=''mailto:support@digioz.com''>support@digioz.com</a>. </p>",
                Keywords = null,
                Description = null,
                Visible = true,
                Timestamp = DateTime.Now
            });

            pages.Add(new Page()
            {
                UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73",
                Title = "Contact",
                Url = "/Home/Contact",
                Body = @"<h2>Contact</h2>
                        <h3>Below is our Contact Information:</h3>
                        <address>One Microsoft Way<br /> Redmond, WA 98052-6399<br /> <abbr title=''Phone''>P:</abbr> 425.555.0100</address>
                        <address><strong>Support:</strong> <a href=''mailto:Support@example.com''>Support@example.com</a><br /> <strong>
                        Marketing:</strong> <a href=''mailto:Marketing@example.com''>Marketing@example.com</a></address>",
                Keywords = null,
                Description = null,
                Visible = true,
                Timestamp = DateTime.Now
            });

            pages.Add(new Page()
            {
                UserId = "b4280b6a-0613-4cbd-a9e6-f1701e926e73",
                Title = "About",
                Url = "/Home/About",
                Body = @"<h2>About</h2>
                        <h3>Some information about us:</h3>
                        <p>Use this area to provide additional information.</p>",
                Keywords = null,
                Description = null,
                Visible = true,
                Timestamp = DateTime.Now
            });

            context.Pages.AddRange(pages);
            context.SaveChanges();

            // Insert Plugin Seed
            var plugins = new List<Plugin>();

            plugins.Add(new Plugin() { Name = "Chat", Dll = null, IsEnabled = true });
            plugins.Add(new Plugin() { Name = "Store", Dll = null, IsEnabled = true });
            plugins.Add(new Plugin() { Name = "Twitter", Dll = null, IsEnabled = false });
            plugins.Add(new Plugin() { Name = "WhoIsOnline", Dll = null, IsEnabled = true });
            plugins.Add(new Plugin() { Name = "SlideShow", Dll = null, IsEnabled = false });
            plugins.Add(new Plugin() { Name = "Comments", Dll = null, IsEnabled = false });
            plugins.Add(new Plugin() { Name = "RSSFeed", Dll = null, IsEnabled = false });
            plugins.Add(new Plugin() { Name = "LatestPictures", Dll = null, IsEnabled = true });

            context.Plugins.AddRange(plugins);
            context.SaveChanges();

            // Insert Zone Seed
            var zones = new List<Zone>();

            zones.Add(new Zone() { Name = "Top", ZoneType = "Module" });
            zones.Add(new Zone() { Name = "TopMenu", ZoneType = "Menu" });
            zones.Add(new Zone() { Name = "Left", ZoneType = "Module" });
            zones.Add(new Zone() { Name = "LeftMenu", ZoneType = "Menu" });
            zones.Add(new Zone() { Name = "BodyTop", ZoneType = "Module" });
            zones.Add(new Zone() { Name = "BodyBottom", ZoneType = "Module" });
            zones.Add(new Zone() { Name = "Bottom", ZoneType = "Module" });

            context.Zones.AddRange(zones);
            context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var context = new digiozPortalContext();

            context.Configs.FromSqlRaw("DELETE FROM Config");
            context.Configs.FromSqlRaw("DELETE FROM Menu");
            context.Configs.FromSqlRaw("DELETE FROM Page");
            context.Configs.FromSqlRaw("DELETE FROM Plugin");
            context.Configs.FromSqlRaw("DELETE FROM Zone");
        }
    }
}
