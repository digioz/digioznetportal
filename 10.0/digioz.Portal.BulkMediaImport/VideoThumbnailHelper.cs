using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Utilities.Helpers;
using Xabe.FFmpeg;

namespace digioz.Portal.BulkMediaImport
{
    public static class VideoThumbnailHelper
    {
        public static async Task<string> GenerateThumbnailAsync(string videoPath, string outputDirectory, int captureTimeSeconds = 10)
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
                
                // Extract a frame at the configured time (or at 10% of video duration if video is too short)
                var duration = videoStream.Duration;
                var minDuration = captureTimeSeconds * 10; // Video should be at least 10x the capture time
                var captureTime = duration.TotalSeconds > minDuration ? TimeSpan.FromSeconds(captureTimeSeconds) : duration.Multiply(0.1);
                
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
                if (!ImageHelper.SaveImageWithCrop(inputPath, width, height, outputPath))
                {
                    throw new InvalidOperationException($"Could not resize thumbnail '{inputPath}'.");
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
