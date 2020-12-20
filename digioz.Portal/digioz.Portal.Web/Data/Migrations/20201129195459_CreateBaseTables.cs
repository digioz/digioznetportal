using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace digioz.Portal.Web.Data.Migrations
{
    public partial class CreateBaseTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    ParentId = table.Column<string>(maxLength: 128, nullable: true),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Username = table.Column<string>(nullable: true),
                    ReferenceId = table.Column<string>(nullable: true),
                    ReferenceType = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    Likes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "CommentConfig",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    ReferenceId = table.Column<string>(nullable: true),
                    ReferenceType = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentLike",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    CommentId = table.Column<string>(maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLike", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    ConfigKey = table.Column<string>(nullable: true),
                    ConfigValue = table.Column<string>(nullable: true),
                    IsEncrypted = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Link",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    LinkCategory = table.Column<int>(nullable: false),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "LogVisitor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IPAddress = table.Column<string>(nullable: true),
                    BrowserType = table.Column<string>(nullable: true),
                    Language = table.Column<string>(nullable: true),
                    IsBot = table.Column<bool>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    ReferringUrl = table.Column<string>(nullable: true),
                    SearchString = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogVisitor", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "MailingList",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(nullable: false),
                    DefaultEmailFrom = table.Column<string>(nullable: false),
                    DefaultFromName = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailingListCampaign",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Subject = table.Column<string>(maxLength: 255, nullable: false),
                    FromName = table.Column<string>(maxLength: 50, nullable: false),
                    FromEmail = table.Column<string>(maxLength: 50, nullable: false),
                    Summary = table.Column<string>(maxLength: 255, nullable: false),
                    Banner = table.Column<string>(maxLength: 255, nullable: true),
                    Body = table.Column<string>(nullable: false),
                    VisitorCount = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingListCampaign", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailingListCampaignRelation",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    MailingListId = table.Column<string>(maxLength: 128, nullable: false),
                    MailingListCampaignId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingListCampaignRelation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailingListSubscriber",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Email = table.Column<string>(maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingListSubscriber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailingListSubscriberRelation",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    MailingListId = table.Column<string>(maxLength: 128, nullable: false),
                    MailingListSubscriberId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingListSubscriberRelation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Location = table.Column<string>(maxLength: 255, nullable: false),
                    Controller = table.Column<string>(maxLength: 50, nullable: true),
                    Action = table.Column<string>(maxLength: 50, nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Target = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Location = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 50, nullable: true),
                    Body = table.Column<string>(maxLength: 50, nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true),
                    DisplayInBox = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    InvoiceNumber = table.Column<string>(maxLength: 20, nullable: true),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    ShippingAddress = table.Column<string>(maxLength: 70, nullable: false),
                    ShippingAddress2 = table.Column<string>(maxLength: 70, nullable: true),
                    ShippingCity = table.Column<string>(maxLength: 40, nullable: false),
                    ShippingState = table.Column<string>(maxLength: 40, nullable: true),
                    ShippingZip = table.Column<string>(maxLength: 30, nullable: false),
                    ShippingCountry = table.Column<string>(maxLength: 50, nullable: false),
                    BillingAddress = table.Column<string>(maxLength: 70, nullable: false),
                    BillingAddress2 = table.Column<string>(maxLength: 70, nullable: true),
                    BillingCity = table.Column<string>(maxLength: 40, nullable: false),
                    BillingState = table.Column<string>(maxLength: 40, nullable: true),
                    BillingZip = table.Column<string>(maxLength: 30, nullable: false),
                    BillingCountry = table.Column<string>(maxLength: 50, nullable: false),
                    Phone = table.Column<string>(maxLength: 30, nullable: true),
                    Email = table.Column<string>(maxLength: 255, nullable: false),
                    Total = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    CCNumber = table.Column<string>(maxLength: 100, nullable: true),
                    CCExp = table.Column<string>(maxLength: 10, nullable: true),
                    CCCardCode = table.Column<string>(maxLength: 10, nullable: true),
                    CCAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    TrxDescription = table.Column<string>(nullable: true),
                    TrxApproved = table.Column<bool>(nullable: false),
                    TrxAuthorizationCode = table.Column<string>(maxLength: 100, nullable: true),
                    TrxMessage = table.Column<string>(nullable: true),
                    TrxResponseCode = table.Column<string>(maxLength: 10, nullable: true),
                    TrxId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    OrderId = table.Column<string>(maxLength: 128, nullable: true),
                    ProductId = table.Column<string>(maxLength: 128, nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Size = table.Column<string>(maxLength: 50, nullable: true),
                    Color = table.Column<string>(maxLength: 50, nullable: true),
                    MaterialType = table.Column<string>(maxLength: 50, nullable: true),
                    Notes = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Page",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Keywords = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Picture",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    AlbumId = table.Column<int>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    Visible = table.Column<bool>(nullable: false),
                    Thumbnail = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Picture", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PictureAlbum",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PictureAlbum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plugin",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Dll = table.Column<string>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugin", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "Poll",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    IsClosed = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    AllowMultipleOptionsVote = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poll", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollAnswer",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    PollId = table.Column<string>(maxLength: 128, nullable: false),
                    Answer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollUsersVote",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: false),
                    PollId = table.Column<string>(maxLength: 128, nullable: false),
                    DateVoted = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollUsersVote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollVote",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<string>(maxLength: 128, nullable: false),
                    PollAnswerId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollVote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    ProductCategoryId = table.Column<string>(maxLength: 128, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Make = table.Column<string>(maxLength: 50, nullable: true),
                    Model = table.Column<string>(maxLength: 50, nullable: true),
                    Sku = table.Column<string>(maxLength: 50, nullable: true),
                    Image = table.Column<string>(maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    Cost = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                    QuantityPerUnit = table.Column<int>(nullable: true),
                    Weight = table.Column<string>(maxLength: 20, nullable: true),
                    Dimensions = table.Column<string>(maxLength: 50, nullable: true),
                    Sizes = table.Column<string>(maxLength: 50, nullable: true),
                    Colors = table.Column<string>(maxLength: 50, nullable: true),
                    MaterialType = table.Column<string>(maxLength: 50, nullable: true),
                    PartNumber = table.Column<string>(maxLength: 50, nullable: true),
                    ShortDescription = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ManufacturerUrl = table.Column<string>(nullable: true),
                    UnitsInStock = table.Column<int>(nullable: true),
                    OutOfStock = table.Column<bool>(maxLength: 50, nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductOption",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    ProductId = table.Column<string>(maxLength: 128, nullable: true),
                    OptionType = table.Column<string>(maxLength: 50, nullable: true),
                    OptionValue = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    MiddleName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    Email = table.Column<string>(maxLength: 255, nullable: true),
                    Birthday = table.Column<DateTime>(nullable: true),
                    BirthdayVisible = table.Column<bool>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(maxLength: 50, nullable: true),
                    State = table.Column<string>(maxLength: 50, nullable: true),
                    Zip = table.Column<string>(maxLength: 20, nullable: true),
                    Country = table.Column<string>(maxLength: 50, nullable: true),
                    Signature = table.Column<string>(maxLength: 255, nullable: true),
                    Avatar = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Url = table.Column<string>(nullable: false),
                    MaxCount = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rss", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    ProductId = table.Column<string>(maxLength: 128, nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    Size = table.Column<bool>(maxLength: 50, nullable: true),
                    Color = table.Column<string>(maxLength: 50, nullable: true),
                    MaterialType = table.Column<string>(maxLength: 50, nullable: true),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlideShow",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Image = table.Column<string>(maxLength: 128, nullable: true),
                    Description = table.Column<string>(maxLength: 128, nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideShow", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Video",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 128, nullable: true),
                    AlbumId = table.Column<int>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    Visible = table.Column<bool>(nullable: false),
                    Thumbnail = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Video", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoAlbum",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    Visible = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAlbum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitorInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IpAddress = table.Column<string>(maxLength: 25, nullable: true),
                    PageUrl = table.Column<string>(nullable: true),
                    ReferringUrl = table.Column<string>(nullable: true),
                    BrowserName = table.Column<string>(maxLength: 100, nullable: true),
                    BrowserType = table.Column<string>(maxLength: 100, nullable: true),
                    BrowserUserAgent = table.Column<string>(nullable: true),
                    BrowserVersion = table.Column<string>(maxLength: 20, nullable: true),
                    IsCrawler = table.Column<bool>(nullable: false),
                    JsVersion = table.Column<string>(nullable: true),
                    OperatingSystem = table.Column<string>(maxLength: 20, nullable: true),
                    Keywords = table.Column<string>(nullable: true),
                    SearchEngine = table.Column<string>(maxLength: 20, nullable: true),
                    Country = table.Column<string>(maxLength: 30, nullable: true),
                    Language = table.Column<string>(maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitorSession",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IpAddress = table.Column<string>(maxLength: 25, nullable: true),
                    PageUrl = table.Column<string>(nullable: true),
                    SessionId = table.Column<string>(nullable: true),
                    Username = table.Column<string>(maxLength: 255, nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zone",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    ZoneType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zone", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Announcement");
            migrationBuilder.DropTable(name: "Chat");
            migrationBuilder.DropTable(name: "Comment");
            migrationBuilder.DropTable(name: "CommentConfig");
            migrationBuilder.DropTable(name: "CommentLike");
            migrationBuilder.DropTable(name: "Config");
            migrationBuilder.DropTable(name: "Link");
            migrationBuilder.DropTable(name: "LinkCategory");
            migrationBuilder.DropTable(name: "Log");
            migrationBuilder.DropTable(name: "LogVisitor");
            migrationBuilder.DropTable(name: "MailingList");
            migrationBuilder.DropTable(name: "MailingListCampaign");
            migrationBuilder.DropTable(name: "MailingListCampaignRelation");
            migrationBuilder.DropTable(name: "MailingListSubscriber");
            migrationBuilder.DropTable(name: "MailingListSubscriberRelation");
            migrationBuilder.DropTable(name: "Menu");
            migrationBuilder.DropTable(name: "Module");
            migrationBuilder.DropTable(name: "Order");
            migrationBuilder.DropTable(name: "OrderDetail");
            migrationBuilder.DropTable(name: "Page");
            migrationBuilder.DropTable(name: "Picture");
            migrationBuilder.DropTable(name: "PictureAlbum");
            migrationBuilder.DropTable(name: "Plugin");
            migrationBuilder.DropTable(name: "Poll");
            migrationBuilder.DropTable(name: "PollAnswer");
            migrationBuilder.DropTable(name: "PollUsersVote");
            migrationBuilder.DropTable(name: "PollVote");
            migrationBuilder.DropTable(name: "Product");
            migrationBuilder.DropTable(name: "ProductCategory");
            migrationBuilder.DropTable(name: "ProductOption");
            migrationBuilder.DropTable(name: "Profile");
            migrationBuilder.DropTable(name: "Rss");
            migrationBuilder.DropTable(name: "ShoppingCart");
            migrationBuilder.DropTable(name: "SlideShow");
            migrationBuilder.DropTable(name: "Video");
            migrationBuilder.DropTable(name: "VideoAlbum");
            migrationBuilder.DropTable(name: "VisitorInfo");
            migrationBuilder.DropTable(name: "VisitorSession");
            migrationBuilder.DropTable(name: "Zone");

        }
    }
}
