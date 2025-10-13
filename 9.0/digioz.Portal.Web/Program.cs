using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(
                        options =>
                        {
                            options.SignIn.RequireConfirmedAccount = false;
                            options.SignIn.RequireConfirmedEmail = false;
                            options.User.RequireUniqueEmail = true;
                            options.Lockout.AllowedForNewUsers = true;
                            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
                            options.Lockout.MaxFailedAccessAttempts = 5;
                        })
    .AddRoles<IdentityRole>()   // Add this line to enable roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add Custom Services
builder.Services.AddDbContext<digiozPortalContext>(
    options => options.UseSqlServer(connectionString),
    optionsLifetime: ServiceLifetime.Scoped);
builder.Services.AddDbContextFactory<digiozPortalContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMailingListSubscriberRelationService, MailingListSubscriberRelationService>();
builder.Services.AddScoped<IPictureAlbumService, PictureAlbumService>();
builder.Services.AddScoped<IMailingListCampaignService, MailingListCampaignService>();
builder.Services.AddScoped<IMailingListSubscriberService, MailingListSubscriberService>();
builder.Services.AddScoped<IAspNetRoleService, AspNetRoleService>();
builder.Services.AddScoped<IAspNetRoleClaimService, AspNetRoleClaimService>();
builder.Services.AddScoped<IAspNetUserService, AspNetUserService>();
builder.Services.AddScoped<IAspNetUserClaimService, AspNetUserClaimService>();
builder.Services.AddScoped<IAspNetUserRoleService, AspNetUserRoleService>();
builder.Services.AddScoped<IAspNetUserTokenService, AspNetUserTokenService>();
builder.Services.AddScoped<ICommentConfigService, CommentConfigService>();
builder.Services.AddScoped<ICommentLikeService, CommentLikeService>();
builder.Services.AddScoped<IConfigService, ConfigService>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<ILinkCategoryService, LinkCategoryService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IMailingListService, MailingListService>();
builder.Services.AddScoped<IMailingListCampaignRelationService, MailingListCampaignRelationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPictureService, PictureService>();
builder.Services.AddScoped<IPluginService, PluginService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IPollAnswerService, PollAnswerService>();
builder.Services.AddScoped<IPollUsersVoteService, PollUsersVoteService>();
builder.Services.AddScoped<IPollVoteService, PollVoteService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductOptionService, ProductOptionService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IRssService, RssService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<ISlideShowService, SlideShowService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IVideoAlbumService, VideoAlbumService>();
builder.Services.AddScoped<IVisitorInfoService, VisitorInfoService>();
builder.Services.AddScoped<IVisitorSessionService, VisitorSessionService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddMemoryCache();

// Helpers: wire CommentsHelper with delegates to avoid Utilities->Dal reference
builder.Services.AddScoped<ICommentsHelper>(sp =>
{
    var configSvc = sp.GetRequiredService<IConfigService>();
    var commentConfigSvc = sp.GetRequiredService<ICommentConfigService>();
    return new CommentsHelper(
        () => configSvc.GetAll(),
        () => commentConfigSvc.GetAll());
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // This will create database if it doesn't exist and apply migrations
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
    }
}

app.UseRouting();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
