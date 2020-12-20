using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using digioz.Portal.Bo;
using System.Configuration;

#nullable disable

namespace digioz.Portal.Dal
{
    public static class ConnectionString
    {
        public static string Value { get; set; }
    }

    public partial class digiozPortalContext : DbContext
    {
        public digiozPortalContext()
        {
        }

        public digiozPortalContext(DbContextOptions<digiozPortalContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<CommentConfig> CommentConfigs { get; set; }
        public virtual DbSet<CommentLike> CommentLikes { get; set; }
        public virtual DbSet<Config> Configs { get; set; }
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<LinkCategory> LinkCategories { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<LogVisitor> LogVisitors { get; set; }
        public virtual DbSet<MailingList> MailingLists { get; set; }
        public virtual DbSet<MailingListCampaign> MailingListCampaigns { get; set; }
        public virtual DbSet<MailingListCampaignRelation> MailingListCampaignRelations { get; set; }
        public virtual DbSet<MailingListSubscriber> MailingListSubscribers { get; set; }
        public virtual DbSet<MailingListSubscriberRelation> MailingListSubscriberRelations { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<Picture> Pictures { get; set; }
        public virtual DbSet<PictureAlbum> PictureAlbums { get; set; }
        public virtual DbSet<Plugin> Plugins { get; set; }
        public virtual DbSet<Poll> Polls { get; set; }
        public virtual DbSet<PollAnswer> PollAnswers { get; set; }
        public virtual DbSet<PollUsersVote> PollUsersVotes { get; set; }
        public virtual DbSet<PollVote> PollVotes { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ProductOption> ProductOptions { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<Rss> Rsses { get; set; }
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public virtual DbSet<SlideShow> SlideShows { get; set; }
        public virtual DbSet<Video> Videos { get; set; }
        public virtual DbSet<VideoAlbum> VideoAlbums { get; set; }
        public virtual DbSet<VisitorInfo> VisitorInfos { get; set; }
        public virtual DbSet<VisitorSession> VisitorSessions { get; set; }
        public virtual DbSet<Zone> Zones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString.Value);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.ToTable("Announcement");

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetRoleClaim>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chat");

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.ParentId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<CommentConfig>(entity =>
            {
                entity.ToTable("CommentConfig");

                entity.Property(e => e.Id).HasMaxLength(128);
            });

            modelBuilder.Entity<CommentLike>(entity =>
            {
                entity.ToTable("CommentLike");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.CommentId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.ToTable("Config");

                entity.Property(e => e.Id).HasMaxLength(128);
            });

            modelBuilder.Entity<Link>(entity =>
            {
                entity.ToTable("Link");
            });

            modelBuilder.Entity<LinkCategory>(entity =>
            {
                entity.ToTable("LinkCategory");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Log");
            });

            modelBuilder.Entity<LogVisitor>(entity =>
            {
                entity.ToTable("LogVisitor");

                entity.Property(e => e.Ipaddress).HasColumnName("IPAddress");
            });

            modelBuilder.Entity<MailingList>(entity =>
            {
                entity.ToTable("MailingList");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Address).IsRequired();

                entity.Property(e => e.DefaultEmailFrom).IsRequired();

                entity.Property(e => e.DefaultFromName).IsRequired();

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<MailingListCampaign>(entity =>
            {
                entity.ToTable("MailingListCampaign");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Banner).HasMaxLength(255);

                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.FromEmail)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FromName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Summary)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<MailingListCampaignRelation>(entity =>
            {
                entity.ToTable("MailingListCampaignRelation");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.MailingListCampaignId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.MailingListId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<MailingListSubscriber>(entity =>
            {
                entity.ToTable("MailingListSubscriber");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MailingListSubscriberRelation>(entity =>
            {
                entity.ToTable("MailingListSubscriberRelation");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.MailingListId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.MailingListSubscriberId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");

                entity.Property(e => e.Action).HasMaxLength(50);

                entity.Property(e => e.Controller).HasMaxLength(50);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<Module>(entity =>
            {
                entity.ToTable("Module");

                entity.Property(e => e.Body).HasMaxLength(50);

                entity.Property(e => e.Location).HasMaxLength(255);

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.BillingAddress)
                    .IsRequired()
                    .HasMaxLength(70);

                entity.Property(e => e.BillingAddress2).HasMaxLength(70);

                entity.Property(e => e.BillingCity)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.BillingCountry)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.BillingState).HasMaxLength(40);

                entity.Property(e => e.BillingZip)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Ccamount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("CCAmount");

                entity.Property(e => e.CccardCode)
                    .HasMaxLength(10)
                    .HasColumnName("CCCardCode");

                entity.Property(e => e.Ccexp)
                    .HasMaxLength(10)
                    .HasColumnName("CCExp");

                entity.Property(e => e.Ccnumber)
                    .HasMaxLength(100)
                    .HasColumnName("CCNumber");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InvoiceNumber).HasMaxLength(20);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(30);

                entity.Property(e => e.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(70);

                entity.Property(e => e.ShippingAddress2).HasMaxLength(70);

                entity.Property(e => e.ShippingCity)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.ShippingCountry)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ShippingState).HasMaxLength(40);

                entity.Property(e => e.ShippingZip)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TrxAuthorizationCode).HasMaxLength(100);

                entity.Property(e => e.TrxResponseCode).HasMaxLength(10);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.MaterialType).HasMaxLength(50);

                entity.Property(e => e.OrderId).HasMaxLength(128);

                entity.Property(e => e.ProductId).HasMaxLength(128);

                entity.Property(e => e.Size).HasMaxLength(50);

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Page>(entity =>
            {
                entity.ToTable("Page");

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<Picture>(entity =>
            {
                entity.ToTable("Picture");

                entity.Property(e => e.AlbumId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<PictureAlbum>(entity =>
            {
                entity.ToTable("PictureAlbum");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Plugin>(entity =>
            {
                entity.ToTable("Plugin");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Poll>(entity =>
            {
                entity.ToTable("Poll");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<PollAnswer>(entity =>
            {
                entity.ToTable("PollAnswer");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.PollId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PollUsersVote>(entity =>
            {
                entity.ToTable("PollUsersVote");

                entity.Property(e => e.PollId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PollVote>(entity =>
            {
                entity.ToTable("PollVote");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.PollAnswerId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Colors).HasMaxLength(50);

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Dimensions).HasMaxLength(50);

                entity.Property(e => e.Image).HasMaxLength(50);

                entity.Property(e => e.Make).HasMaxLength(50);

                entity.Property(e => e.MaterialType).HasMaxLength(50);

                entity.Property(e => e.Model).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PartNumber).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductCategoryId).HasMaxLength(128);

                entity.Property(e => e.ShortDescription).HasMaxLength(255);

                entity.Property(e => e.Sizes).HasMaxLength(50);

                entity.Property(e => e.Sku).HasMaxLength(50);

                entity.Property(e => e.Weight).HasMaxLength(20);
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ProductOption>(entity =>
            {
                entity.ToTable("ProductOption");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.OptionType).HasMaxLength(50);

                entity.Property(e => e.OptionValue).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasMaxLength(128);
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.ToTable("Profile");

                entity.Property(e => e.Avatar).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.Property(e => e.Signature).HasMaxLength(255);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.Zip).HasMaxLength(20);
            });

            modelBuilder.Entity<Rss>(entity =>
            {
                entity.ToTable("Rss");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Url).IsRequired();
            });

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.ToTable("ShoppingCart");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.MaterialType).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<SlideShow>(entity =>
            {
                entity.ToTable("SlideShow");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.Image).HasMaxLength(128);
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("Video");

                entity.Property(e => e.AlbumId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<VideoAlbum>(entity =>
            {
                entity.ToTable("VideoAlbum");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<VisitorInfo>(entity =>
            {
                entity.ToTable("VisitorInfo");

                entity.Property(e => e.BrowserName).HasMaxLength(100);

                entity.Property(e => e.BrowserType).HasMaxLength(100);

                entity.Property(e => e.BrowserVersion).HasMaxLength(20);

                entity.Property(e => e.Country).HasMaxLength(30);

                entity.Property(e => e.IpAddress).HasMaxLength(25);

                entity.Property(e => e.Language).HasMaxLength(100);

                entity.Property(e => e.OperatingSystem).HasMaxLength(20);

                entity.Property(e => e.SearchEngine).HasMaxLength(20);
            });

            modelBuilder.Entity<VisitorSession>(entity =>
            {
                entity.ToTable("VisitorSession");

                entity.Property(e => e.IpAddress).HasMaxLength(25);

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            modelBuilder.Entity<Zone>(entity =>
            {
                entity.ToTable("Zone");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.ZoneType).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
