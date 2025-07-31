using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace digioz.Portal.Utilities.Helpers
{
    public class ImageSaveData
    {
        public Image Image { get; set; }
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

                using (var image = (Image)isd.Image.Clone())
                {
                    if (SaveJigsawImage(image, isd.FullMaxWidth, isd.FullMaxHeight, fullFilePath))
                    {
                        using (var smallImage = (Image)isd.Image.Clone())
                        {
                            if (SaveJigsawImage(smallImage, isd.SmallMaxWidth, isd.SmallMaxHeight, smallFilePath))
                            {
                                using (var tinyImage = (Image)isd.Image.Clone())
                                {
                                    if (SaveImageWithCrop(tinyImage, isd.TinyMaxWidth, isd.TinyMaxHeight, tinyFilePath))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception here if needed
            }
            return false;
        }

        public static bool SaveJigsawImage(Image image, int maxWidth, int maxHeight, string filePath)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().First(codecInfo => codecInfo.MimeType == "image/jpeg");
            Bitmap bitmap = null;
            Image finalImage = image;

            try
            {
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    double divisor;
                    if (image.Width >= image.Height)
                    {
                        divisor = (double)image.Width / maxWidth;
                        int height = (int)Math.Round(image.Height / divisor);
                        bitmap = new Bitmap(maxWidth, height);
                    }
                    else
                    {
                        divisor = (double)image.Height / maxHeight;
                        int width = (int)Math.Round(image.Width / divisor);
                        bitmap = new Bitmap(width, maxHeight);
                    }

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    }
                    finalImage = bitmap;
                }

                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                    finalImage.Save(filePath, jpgInfo, encParams);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return false;
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }
        }

        public static bool SaveImageWithCrop(Image image, int maxWidth, int maxHeight, string filePath)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().First(codecInfo => codecInfo.MimeType == "image/jpeg");
            Bitmap bitmap = null;
            Image finalImage = image;

            try
            {
                int left = 0, top = 0, srcWidth = maxWidth, srcHeight = maxHeight;
                bitmap = new Bitmap(maxWidth, maxHeight);
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

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(left, top, srcWidth, srcHeight), GraphicsUnit.Pixel);
                }
                finalImage = bitmap;

                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                    finalImage.Save(filePath, jpgInfo, encParams);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return false;
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }
        }
    }
}