using System;
using System.IO;
using SkiaSharp;

namespace digioz.Portal.Utilities.Helpers
{
    public class ImageSaveData
    {
        public string ImagePath { get; set; }
        public int FullMaxWidth { get; set; }
        public int FullMaxHeight { get; set; }
        public int SmallMaxWidth { get; set; }
        public int SmallMaxHeight { get; set; }
        public int TinyMaxWidth { get; set; }
        public int TinyMaxHeight { get; set; }
        public string FileName { get; set; }
        public string FullImageFolder { get; set; }
        public string SmallImageFolder { get; set; }
        public string TinyImageFolder { get; set; }
    }

    public static class ImageHelper
    {
        public static bool SaveJigsawImage(ImageSaveData isd)
        {
            try
            {
                string fullFilePath = Path.Combine(isd.FullImageFolder, isd.FileName);
                string smallFilePath = Path.Combine(isd.SmallImageFolder, isd.FileName);
                string tinyFilePath = Path.Combine(isd.TinyImageFolder, isd.FileName);

                if (SaveJigsawImage(isd.ImagePath, isd.FullMaxWidth, isd.FullMaxHeight, fullFilePath))
                {
                    if (SaveJigsawImage(isd.ImagePath, isd.SmallMaxWidth, isd.SmallMaxHeight, smallFilePath))
                    {
                        if (SaveImageWithCrop(isd.ImagePath, isd.TinyMaxWidth, isd.TinyMaxHeight, tinyFilePath))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception here if needed
            }
            return false;
        }

        public static bool SaveJigsawImage(string imagePath, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                using var image = SKBitmap.Decode(imagePath);
                return image != null && SaveJigsawImage(image, maxWidth, maxHeight, filePath);
            }
            catch (Exception)
            {
                // Log exception here if needed
                return false;
            }
        }

        public static bool SaveJigsawImage(byte[] imageData, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                using var image = SKBitmap.Decode(imageData);
                return image != null && SaveJigsawImage(image, maxWidth, maxHeight, filePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SaveImageWithCrop(string imagePath, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                using var image = SKBitmap.Decode(imagePath);
                return image != null && SaveImageWithCrop(image, maxWidth, maxHeight, filePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SaveImageWithCrop(byte[] imageData, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                using var image = SKBitmap.Decode(imageData);
                return image != null && SaveImageWithCrop(image, maxWidth, maxHeight, filePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static bool SaveImageWithCrop(SKBitmap image, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                int left = 0, top = 0, srcWidth = maxWidth, srcHeight = maxHeight;
                double croppedHeightToWidth = (double)maxHeight / maxWidth;
                double croppedWidthToHeight = (double)maxWidth / maxHeight;

                if (image.Width > image.Height)
                {
                    srcWidth = (int)Math.Round(image.Height * croppedWidthToHeight);
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
                    srcHeight = (int)Math.Round(image.Width * croppedHeightToWidth);
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

                using var croppedImage = new SKBitmap(new SKImageInfo(maxWidth, maxHeight, image.ColorType, image.AlphaType));
                using var canvas = new SKCanvas(croppedImage);
                var source = new SKRect(left, top, left + srcWidth, top + srcHeight);
                var destination = new SKRect(0, 0, maxWidth, maxHeight);
                canvas.DrawBitmap(image, source, destination, new SKSamplingOptions(SKCubicResampler.Mitchell));
                canvas.Flush();

                return SaveJpeg(croppedImage, filePath);
            }
            catch (Exception)
            {
                // Log exception here if needed
                return false;
            }
        }

        private static bool SaveJigsawImage(SKBitmap image, int maxWidth, int maxHeight, string filePath)
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                return SaveJpeg(image, filePath);
            }

            double divisor;
            int newWidth;
            int newHeight;

            if (image.Width >= image.Height)
            {
                divisor = (double)image.Width / maxWidth;
                newHeight = (int)Math.Round(image.Height / divisor);
                newWidth = maxWidth;
            }
            else
            {
                divisor = (double)image.Height / maxHeight;
                newWidth = (int)Math.Round(image.Width / divisor);
                newHeight = maxHeight;
            }

            using var resizedImage = image.Resize(
                new SKImageInfo(newWidth, newHeight, image.ColorType, image.AlphaType),
                new SKSamplingOptions(SKCubicResampler.Mitchell));

            return resizedImage != null && SaveJpeg(resizedImage, filePath);
        }

        private static bool SaveJpeg(SKBitmap image, string filePath)
        {
            using var outputImage = SKImage.FromBitmap(image);
            using var encodedImage = outputImage.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var output = File.Create(filePath);
            encodedImage.SaveTo(output);
            return true;
        }
    }
}