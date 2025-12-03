using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Data;
using digioz.Portal.Web.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Web.Hubs;
using digioz.Portal.EmailProviders.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure DB logger provider from configuration (appsettings.json: DbLogger)
builder.Services.Configure<DbLoggerOptions>(builder.Configuration.GetSection("DbLogger"));
builder.Logging.AddDbLogger();

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
    .AddRoles<IdentityRole>() // Add this line to enable roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure application cookie with security stamp validation
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    
    // Validate security stamp to detect compromised/deleted accounts
    // This ensures deleted users are signed out quickly
    options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});

// Configure security stamp validator
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Validate security stamp every 30 minutes
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

// Admin authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrator"));
});

// Data context for Portal tables
builder.Services.AddDbContext<digiozPortalContext>(options =>
    options.UseSqlServer(connectionString));

// Register Portal Services (scoped lifetime)
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IMailingListCampaignService, MailingListCampaignService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IPictureAlbumService, PictureAlbumService>();
builder.Services.AddScoped<IMailingListSubscriberRelationService, MailingListSubscriberRelationService>();
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
builder.Services.AddScoped<IPrivateMessageService, PrivateMessageService>();
builder.Services.AddScoped<IThemeService, ThemeService>();

// Register Email Provider Services
builder.Services.AddEmailProviders();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

builder.Services.AddMemoryCache();

// Recaptcha verification needs HttpClient
builder.Services.AddHttpClient();

// Add HttpContextAccessor for accessing HttpContext in view components
builder.Services.AddHttpContextAccessor();

// Session for capturing SessionId in visitor logs
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    var sessionConfig = builder.Configuration.GetSection("Session");
    var cookieName = sessionConfig["CookieName"] ?? ".digiozPortal.Session";
    var timeoutMinutes = int.TryParse(sessionConfig["IdleTimeoutMinutes"], out var minutes) ? minutes : 20;
    
    options.IdleTimeout = TimeSpan.FromMinutes(timeoutMinutes);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = cookieName;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP in development
});

// Helpers: wire CommentsHelper with delegates to avoid Utilities->Dal reference
builder.Services.AddScoped<ICommentsHelper>(sp =>
{
    var configSvc = sp.GetRequiredService<IConfigService>();
    var commentConfigSvc = sp.GetRequiredService<ICommentConfigService>();
    return new CommentsHelper(
        () => configSvc.GetAll(),
        () => commentConfigSvc.GetAll());
});

// UserHelper registration (delegate pulls from IAspNetUserService)
builder.Services.AddScoped<IUserHelper>(sp =>
{
    var userSvc = sp.GetRequiredService<IAspNetUserService>();
    return new UserHelper(() => userSvc.GetAll());
});

// Razor Pages with convention to authorize entire Admin area
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
});

// Visitor logging for all Razor Pages
builder.Services.AddVisitorInfoLogging();

// SignalR registration
builder.Services.AddSignalR();

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

// Ensure databases are created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Identity DB
        var identityContext = services.GetRequiredService<ApplicationDbContext>();
        identityContext.Database.Migrate();

        // Main portal DB (digiozPortalContext)
        var portalContext = services.GetRequiredService<digiozPortalContext>();
        portalContext.Database.EnsureCreated();
    }
    catch (SqlException ex)
    {
        var message = "FATAL: SQL Server error occurred during database initialization. Application cannot start.";
        logger.LogCritical(ex, message);
        StartupFileLogger.LogCritical(ex, message);
        throw;
    }
    catch (DbUpdateException ex)
    {
        var message = "FATAL: Database update error occurred during initialization. Application cannot start.";
        logger.LogCritical(ex, message);
        StartupFileLogger.LogCritical(ex, message);
        throw;
    }
    catch (InvalidOperationException ex)
    {
        var message = "FATAL: Configuration error occurred during database initialization. Application cannot start.";
        logger.LogCritical(ex, message);
        StartupFileLogger.LogCritical(ex, message);
        throw;
    }
    catch (Exception ex)
    {
        var message = "FATAL: Unexpected error occurred during database initialization. Application cannot start.";
        logger.LogCritical(ex, message);
        StartupFileLogger.LogCritical(ex, message);
        throw;
    }
}

app.UseSession();

// Use traditional static files middleware without fingerprinting
// This allows direct file replacement on the server without cache-busting hashes
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Remove MapStaticAssets() and use traditional MapRazorPages()
app.MapRazorPages();

// Map ChatHub endpoint
app.MapHub<ChatHub>("/chatHub");

app.Run();
