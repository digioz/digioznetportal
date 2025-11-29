using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Xabe.FFmpeg;

namespace digioz.Portal.BulkMediaImport
{
    public static class VideoThumbnailHelper
    {
        public static async Task<string> GenerateThumbnailAsync(string videoPath, string outputDirectory)
        {
            try
            {
                var videoInfo = await FFmpeg.GetMediaInfo(videoPath);
                var videoStream = videoInfo.VideoStreams.FirstOrDefault();
                
                if (videoStream == null)
                {
                    throw new InvalidOperationException("No video stream found in the video file.");
                }
                
                // Generate a unique filename for the thumbnail
                var thumbnailFileName = Path.GetFileNameWithoutExtension(videoPath) + "_thumb.jpg";
                var thumbnailPath = Path.Combine(outputDirectory, thumbnailFileName);
                
                // Extract a frame at 1 second (or at 10% of video duration if shorter)
                var duration = videoStream.Duration;
                var captureTime = duration.TotalSeconds > 10 ? TimeSpan.FromSeconds(1) : duration.Multiply(0.1);
                
                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, thumbnailPath, captureTime);
                await conversion.Start();
                
                return thumbnailPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating thumbnail: {ex.Message}");
                throw;
            }
        }
        
        public static void ResizeAndCropThumbnail(string inputPath, string outputPath, int width, int height)
        {
            try
            {
                using (var image = Image.Load(inputPath))
                {
                    int left = 0;
                    int top = 0;
                    int srcWidth = width;
                    int srcHeight = height;
                    double croppedHeightToWidth = (double)height / width;
                    double croppedWidthToHeight = (double)width / height;

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
                        .Resize(width, height));

                    var encoder = new JpegEncoder { Quality = 100 };
                    image.Save(outputPath, encoder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resizing thumbnail: {ex.Message}");
                throw;
            }
        }
    }
}
