using digioz.Portal.Dal;
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
builder.Services.AddDbContextFactory<digiozPortalContext>(options => options.UseSqlServer(connectionString));

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
