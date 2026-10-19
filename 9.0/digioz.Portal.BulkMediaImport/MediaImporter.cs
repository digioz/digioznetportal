using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace digioz.Portal.BulkMediaImport
{
    public class MediaImporter
    {
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;
        private readonly IPictureAlbumService _pictureAlbumService;
        private readonly IVideoAlbumService _videoAlbumService;
        private readonly int _videoThumbnailCaptureTimeSeconds;

        public MediaImporter(
            IPictureService pictureService,
            IVideoService videoService,
            IPictureAlbumService pictureAlbumService,
            IVideoAlbumService videoAlbumService,
            IConfiguration configuration)
        {
            _pictureService = pictureService;
            _videoService = videoService;
            _pictureAlbumService = pictureAlbumService;
            _videoAlbumService = videoAlbumService;
            
            // Read the video thumbnail capture time from configuration, default to 10 seconds
            _videoThumbnailCaptureTimeSeconds = configuration.GetValue<int>("VideoThumbnail:CaptureTimeSeconds", 10);
        }

        public async Task<(int successCount, int errorCount)> ImportPicturesAsync(
            int albumId,
            string sourceDirectory,
            string baseImageDirectory,
            bool useFileNameAsDescription,
            string? ownerId = null)
        {
            int successCount = 0;
            int errorCount = 0;

            // Validate album exists
            var album = _pictureAlbumService.GetAll().FirstOrDefault(a => a.Id == albumId);
            if (album == null)
            {
                throw new ArgumentException($"Album with ID {albumId} not found.");
            }

            Console.WriteLine($"Importing pictures to album: {album.Name}");
            if (!string.IsNullOrEmpty(ownerId))
            {
                Console.WriteLine($"Owner: {ownerId}");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
            var files = Directory.GetFiles(sourceDirectory)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            Console.WriteLine($"Found {files.Count} image file(s) to import.");

            // Create directories
            var fullDir = Path.Combine(baseImageDirectory, "Pictures", "Full");
            var thumbDir = Path.Combine(baseImageDirectory, "Pictures", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            foreach (var sourceFile in files)
            {
                try
                {
                    var ext = Path.GetExtension(sourceFile).ToLowerInvariant();
                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var fullPath = Path.Combine(fullDir, fileName);
                    var thumbPath = Path.Combine(thumbDir, fileName);

                    // Copy original file
                    File.Copy(sourceFile, fullPath, overwrite: false);

                    // Create thumbnail (150x150 crop) using ImageSharp
                    using (var image = Image.Load(fullPath))
                    {
                        SaveImageWithCrop(image, 150, 150, thumbPath);
                    }

                    // Get description
                    var description = useFileNameAsDescription
                        ? Path.GetFileNameWithoutExtension(sourceFile)
                        : string.Empty;

                    // Create database record
                    var picture = new Picture
                    {
                        Filename = fileName,
                        Thumbnail = fileName,
                        Description = description,
                        AlbumId = albumId,
                        UserId = ownerId,
                        Visible = true,
                        Approved = true,
                        Timestamp = DateTime.UtcNow
                    };

                    _pictureService.Add(picture);
                    successCount++;
                    Console.WriteLine($"  ? Imported: {Path.GetFileName(sourceFile)}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"  ? Failed to import {Path.GetFileName(sourceFile)}: {ex.Message}");
                }
            }

            return (successCount, errorCount);
        }

        public async Task<(int successCount, int errorCount)> ImportVideosAsync(
            int albumId,
            string sourceDirectory,
            string baseImageDirectory,
            bool useFileNameAsDescription,
            string? ownerId = null)
        {
            int successCount = 0;
            int errorCount = 0;

            // Validate album exists
            var album = _videoAlbumService.GetAll().FirstOrDefault(a => a.Id == albumId);
            if (album == null)
            {
                throw new ArgumentException($"Album with ID {albumId} not found.");
            }

            Console.WriteLine($"Importing videos to album: {album.Name}");
            if (!string.IsNullOrEmpty(ownerId))
            {
                Console.WriteLine($"Owner: {ownerId}");
            }

            var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv", ".mpg", ".mpeg", ".wmv" };
            var files = Directory.GetFiles(sourceDirectory)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            Console.WriteLine($"Found {files.Count} video file(s) to import.");

            // Create directories
            var fullDir = Path.Combine(baseImageDirectory, "Videos", "Full");
            var thumbDir = Path.Combine(baseImageDirectory, "Videos", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            foreach (var sourceFile in files)
            {
                try
                {
                    var videoExt = Path.GetExtension(sourceFile).ToLowerInvariant();
                    var guid = Guid.NewGuid().ToString("N");
                    var videoFileName = guid + videoExt;
                    var thumbFileName = guid + ".jpg";
                    var videoFullPath = Path.Combine(fullDir, videoFileName);
                    var thumbPath = Path.Combine(thumbDir, thumbFileName);

                    Console.WriteLine($"  Processing: {Path.GetFileName(sourceFile)}");

                    // Copy video file
                    File.Copy(sourceFile, videoFullPath, overwrite: false);
                    Console.WriteLine($"    - Video copied");

                    // Generate thumbnail from video
                    try
                    {
                        var tempThumbPath = await VideoThumbnailHelper.GenerateThumbnailAsync(videoFullPath, thumbDir, _videoThumbnailCaptureTimeSeconds);
                        
                        // Resize and crop the thumbnail to 150x150
                        VideoThumbnailHelper.ResizeAndCropThumbnail(tempThumbPath, thumbPath, 150, 150);
                        
                        // Delete the temporary full-size thumbnail
                        if (File.Exists(tempThumbPath) && tempThumbPath != thumbPath)
                        {
                            File.Delete(tempThumbPath);
                        }
                        
                        Console.WriteLine($"    - Thumbnail generated from video");
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Cannot find FFmpeg"))
                        {
                            Console.WriteLine($"    - FFmpeg not available, using placeholder thumbnail");
                        }
                        else
                        {
                            Console.WriteLine($"    - Could not generate thumbnail: {ex.Message}");
                        }
                        // Create a placeholder thumbnail if video thumbnail generation fails
                        CreatePlaceholderThumbnail(thumbPath);
                    }

                    // Get description
                    var description = useFileNameAsDescription
                        ? Path.GetFileNameWithoutExtension(sourceFile)
                        : string.Empty;

                    // Create database record
                    var video = new Video
                    {
                        Filename = videoFileName,
                        Thumbnail = thumbFileName,
                        Description = description,
                        AlbumId = albumId,
                        UserId = ownerId,
                        Visible = true,
                        Approved = true,
                        Timestamp = DateTime.UtcNow
                    };

                    _videoService.Add(video);
                    successCount++;
                    Console.WriteLine($"  ? Imported: {Path.GetFileName(sourceFile)}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"  ? Failed to import {Path.GetFileName(sourceFile)}: {ex.Message}");
                }
            }

            return (successCount, errorCount);
        }

        private void SaveImageWithCrop(Image image, int maxWidth, int maxHeight, string filePath)
        {
            int left = 0;
            int top = 0;
            int srcWidth = maxWidth;
            int srcHeight = maxHeight;
            double croppedHeightToWidth = (double)maxHeight / maxWidth;
            double croppedWidthToHeight = (double)maxWidth / maxHeight;

            if (image.Width > image.Height)
            {
                srcWidth = (int)(Math.Round(image.Height * croppedWidthToHeight));
                if (srcWidth < image.Width)
                {
                    srcHeight = image.Height;
                    left = (image.Width - srcWidth) / 2;
                }
                else
                {
                    srcHeight = (int)Math.Round(image.Height * ((double)image.Width / srcWidth));
                    srcWidth = image.Width;
                    top = (image.Height - srcHeight) / 2;
                }
            }
            else
            {
                srcHeight = (int)(Math.Round(image.Width * croppedHeightToWidth));
                if (srcHeight < image.Height)
                {
                    srcWidth = image.Width;
                    top = (image.Height - srcHeight) / 2;
                }
                else
                {
                    srcWidth = (int)Math.Round(image.Width * ((double)image.Height / srcHeight));
                    srcHeight = image.Height;
                    left = (image.Width - srcWidth) / 2;
                }
            }

            image.Mutate(x => x
                .Crop(new Rectangle(left, top, srcWidth, srcHeight))
                .Resize(maxWidth, maxHeight));

            var encoder = new JpegEncoder { Quality = 100 };
            image.Save(filePath, encoder);
        }

        private void CreatePlaceholderThumbnail(string outputPath)
        {
            // Create a simple 150x150 black placeholder image
            using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(150, 150))
            {
                image.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.Black));
                var encoder = new JpegEncoder { Quality = 100 };
                image.Save(outputPath, encoder);
            }
        }
    }
}
