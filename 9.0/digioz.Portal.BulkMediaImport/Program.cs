using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace digioz.Portal.BulkMediaImport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("DigiOz Portal Bulk Import starting ...");
            Console.WriteLine();

            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Set up dependency injection
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configuration)
                    .AddDbContext<digiozPortalContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("DefaultConnection"),
                            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null)))
                    .AddScoped<IPictureService, PictureService>()
                    .AddScoped<IVideoService, VideoService>()
                    .AddScoped<IPictureAlbumService, PictureAlbumService>()
                    .AddScoped<IVideoAlbumService, VideoAlbumService>()
                    .AddScoped<IAspNetUserService, AspNetUserService>()
                    .AddScoped<IAspNetUserRoleService, AspNetUserRoleService>()
                    .AddScoped<IAspNetRoleService, AspNetRoleService>()
                    .AddScoped<MediaImporter>()
                    .BuildServiceProvider();

                // Initialize FFmpeg (required for video thumbnail generation)
                Console.WriteLine("Checking FFmpeg availability...");
                bool ffmpegAvailable = false;
                try
                {
                    // Try to find FFmpeg in common locations
                    var possiblePaths = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin"),
                        @"C:\ffmpeg\bin"
                    };
                    
                    string ffmpegPath = null;
                    foreach (var path in possiblePaths)
                    {
                        if (Directory.Exists(path))
                        {
                            var ffmpegExe = Path.Combine(path, "ffmpeg.exe");
                            if (File.Exists(ffmpegExe))
                            {
                                ffmpegPath = path;
                                break;
                            }
                        }
                    }
                    
                    if (ffmpegPath != null)
                    {
                        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(ffmpegPath);
                        Console.WriteLine($"✓ FFmpeg found at: {ffmpegPath}");
                        ffmpegAvailable = true;
                    }
                    else
                    {
                        Console.WriteLine("⚠ FFmpeg not found in common locations.");
                        Console.WriteLine();
                        Console.WriteLine("To enable video thumbnail generation:");
                        Console.WriteLine("  1. Download FFmpeg from: https://ffmpeg.org/download.html");
                        Console.WriteLine("  2. Extract it to one of these locations:");
                        Console.WriteLine($"     - {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg")}");
                        Console.WriteLine("     - C:\\ffmpeg");
                        Console.WriteLine("  3. Or add FFmpeg to your system PATH");
                        Console.WriteLine();
                        Console.WriteLine("Videos will still be imported, but placeholder thumbnails will be used.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Warning: Could not set FFmpeg path: {ex.Message}");
                    Console.WriteLine("Videos will still be imported, but placeholder thumbnails will be used.");
                }
                Console.WriteLine();

                using (var scope = serviceProvider.CreateScope())
                {
                    var importer = scope.ServiceProvider.GetRequiredService<MediaImporter>();
                    
                    // Get media type
                    Console.WriteLine("Select media type:");
                    Console.WriteLine("  1 - Pictures");
                    Console.WriteLine("  2 - Videos");
                    Console.Write("Enter your choice (1 or 2): ");
                    var mediaTypeInput = Console.ReadLine()?.Trim();
                    
                    if (mediaTypeInput != "1" && mediaTypeInput != "2")
                    {
                        Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                        return;
                    }
                    
                    bool isPictures = mediaTypeInput == "1";
                    string mediaType = isPictures ? "Pictures" : "Videos";
                    
                    Console.WriteLine();
                    
                    // Get album ID
                    Console.WriteLine($"Available {mediaType} Albums:");
                    
                    if (isPictures)
                    {
                        var pictureAlbumService = scope.ServiceProvider.GetRequiredService<IPictureAlbumService>();
                        var albums = pictureAlbumService.GetAll().OrderBy(a => a.Name).ToList();
                        
                        if (albums.Count == 0)
                        {
                            Console.WriteLine("No picture albums found. Please create an album first.");
                            return;
                        }
                        
                        foreach (var album in albums)
                        {
                            Console.WriteLine($"  {album.Id} - {album.Name}");
                        }
                    }
                    else
                    {
                        var videoAlbumService = scope.ServiceProvider.GetRequiredService<IVideoAlbumService>();
                        var albums = videoAlbumService.GetAll().OrderBy(a => a.Name).ToList();
                        
                        if (albums.Count == 0)
                        {
                            Console.WriteLine("No video albums found. Please create an album first.");
                            return;
                        }
                        
                        foreach (var album in albums)
                        {
                            Console.WriteLine($"  {album.Id} - {album.Name}");
                        }
                    }
                    
                    Console.Write($"Enter the Album ID for the imported files: ");
                    if (!int.TryParse(Console.ReadLine()?.Trim(), out int albumId) || albumId <= 0)
                    {
                        Console.WriteLine("Invalid Album ID.");
                        return;
                    }
                    
                    Console.WriteLine();
                    
                    // Get source directory
                    Console.Write("Enter the input directory to get the media from: ");
                    var sourceDirectory = Console.ReadLine()?.Trim();
                    
                    if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
                    {
                        Console.WriteLine("Invalid or non-existent directory.");
                        return;
                    }
                    
                    Console.WriteLine();
                    
                    // Get base image directory
                    Console.Write("Enter the save img base directory: ");
                    var baseImageDirectory = Console.ReadLine()?.Trim();
                    
                    if (string.IsNullOrWhiteSpace(baseImageDirectory))
                    {
                        Console.WriteLine("Invalid base directory.");
                        return;
                    }
                    
                    // Create base directory if it doesn't exist
                    Directory.CreateDirectory(baseImageDirectory);
                    
                    Console.WriteLine();
                    
                    // Get description option
                    Console.WriteLine("Description option:");
                    Console.WriteLine("  1 - Use file name as description");
                    Console.WriteLine("  2 - Leave description blank");
                    Console.Write("Enter your choice (1 or 2): ");
                    var descriptionInput = Console.ReadLine()?.Trim();
                    
                    if (descriptionInput != "1" && descriptionInput != "2")
                    {
                        Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                        return;
                    }
                    
                    bool useFileNameAsDescription = descriptionInput == "1";
                    
                    Console.WriteLine();
                    
                    // Get owner user
                    Console.WriteLine("Select owner for imported media:");
                    Console.WriteLine("Available Admin Users:");
                    
                    var userService = scope.ServiceProvider.GetRequiredService<IAspNetUserService>();
                    var userRoleService = scope.ServiceProvider.GetRequiredService<IAspNetUserRoleService>();
                    var roleService = scope.ServiceProvider.GetRequiredService<IAspNetRoleService>();
                    
                    // Get Administrator role
                    var adminRole = roleService.GetAll().FirstOrDefault(r => r.Name == "Administrator");
                    
                    var adminUsers = new System.Collections.Generic.List<digioz.Portal.Bo.AspNetUser>();
                    if (adminRole != null)
                    {
                        // Get all user-role relationships for admin role
                        var adminUserRoles = userRoleService.GetAll().Where(ur => ur.RoleId == adminRole.Id).ToList();
                        
                        // Get the actual users
                        foreach (var userRole in adminUserRoles)
                        {
                            var user = userService.Get(userRole.UserId);
                            if (user != null)
                            {
                                adminUsers.Add(user);
                            }
                        }
                    }
                    
                    if (adminUsers.Count == 0)
                    {
                        Console.WriteLine("  No admin users found.");
                    }
                    else
                    {
                        foreach (var user in adminUsers.OrderBy(u => u.UserName))
                        {
                            Console.WriteLine($"  {user.UserName} ({user.Email})");
                        }
                    }
                    Console.WriteLine();
                    
                    Console.Write("Enter the UserName of the user to set as owner (or leave blank for no owner): ");
                    var ownerUsername = Console.ReadLine()?.Trim();
                    
                    string ownerId = null;
                    if (!string.IsNullOrWhiteSpace(ownerUsername))
                    {
                        var selectedUser = adminUsers.FirstOrDefault(u => 
                            u.UserName.Equals(ownerUsername, StringComparison.OrdinalIgnoreCase));
                        
                        if (selectedUser != null)
                        {
                            ownerId = selectedUser.Id;
                            Console.WriteLine($"Owner set to: {selectedUser.UserName}");
                        }
                        else
                        {
                            Console.WriteLine($"Warning: User '{ownerUsername}' not found in admin list. Proceeding without owner.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No owner specified.");
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine("=".PadRight(60, '='));
                    Console.WriteLine("Starting import...");
                    Console.WriteLine("=".PadRight(60, '='));
                    Console.WriteLine();
                    
                    // Perform import
                    int successCount, errorCount;
                    
                    if (isPictures)
                    {
                        (successCount, errorCount) = await importer.ImportPicturesAsync(
                            albumId,
                            sourceDirectory,
                            baseImageDirectory,
                            useFileNameAsDescription,
                            ownerId);
                    }
                    else
                    {
                        (successCount, errorCount) = await importer.ImportVideosAsync(
                            albumId,
                            sourceDirectory,
                            baseImageDirectory,
                            useFileNameAsDescription,
                            ownerId);
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine("=".PadRight(60, '='));
                    Console.WriteLine($"Import completed: {successCount} successful, {errorCount} failed");
                    Console.WriteLine("=".PadRight(60, '='));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack Trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("The import has completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
