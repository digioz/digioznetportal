using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace digioz.Portal.Web.Helpers
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
        public string SmallIMageFolder { get; set; }
        public string TinyImageFolder { get; set; }
    }

    public static class ImageHelper
    {

        public static bool SaveJigsawImage(ImageSaveData isd)
        {
            try
            {
                string fullFilePath = Path.Combine(isd.FullImageFolder, isd.FileName);
                string smallFilePath = Path.Combine(isd.SmallIMageFolder, isd.FileName);
                string tinyFilePath = Path.Combine(isd.TinyImageFolder, isd.FileName);
                if (SaveJigsawImage(isd.Image, isd.FullMaxWidth, isd.FullMaxHeight, fullFilePath))
                {
                    if (SaveJigsawImage(isd.Image, isd.SmallMaxWidth, isd.SmallMaxHeight, smallFilePath))
                    {
                        if (SaveImageWithCrop(isd.Image, isd.TinyMaxWidth, isd.TinyMaxHeight, tinyFilePath))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { };
            return false;
        }


        public static bool SaveJigsawImage(Image Image, int MaxWidth, int MaxHeight, string filePath)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
            Image finalImage = Image;
            System.Drawing.Bitmap bitmap = null;
            if (Image.Width > MaxWidth || Image.Height > MaxHeight)
            {
                double divisor;
                try
                {
                    if (Image.Width >= Image.Height)
                    {
                        divisor = (double)Image.Width / MaxWidth;
                        int height = (int)(Math.Round(Image.Height / divisor));
                        bitmap = new System.Drawing.Bitmap(MaxWidth, height);
                    }
                    else
                    {
                        divisor = (double)Image.Height / MaxHeight;
                        int width = (int)(Math.Round(Image.Width / divisor));
                        bitmap = new System.Drawing.Bitmap(width, MaxHeight);
                    }
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(Image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(0, 0, Image.Width, Image.Height), GraphicsUnit.Pixel);
                    }
                    finalImage = bitmap;
                }
                catch { }
            }
            try
            {
                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    //quality should be in the range [0..100] .. 100 for max, 0 for min (0 best compression)
                    finalImage.Save(filePath, jpgInfo, encParams);
                    return true;
                }
            }
            catch { }
            if (bitmap != null)
            {
                bitmap.Dispose();
            }
            return false;
        }

        public static bool SaveImageWithCrop(Image image, int maxWidth, int maxHeight, string filePath)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
            Image finalImage = image;
            System.Drawing.Bitmap bitmap = null;
            try
            {
                int left = 0;
                int top = 0;
                int srcWidth = maxWidth;
                int srcHeight = maxHeight;
                bitmap = new System.Drawing.Bitmap(maxWidth, maxHeight);
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
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(left, top, srcWidth, srcHeight), GraphicsUnit.Pixel);
                }
                finalImage = bitmap;
            }
            catch { }
            try
            {
                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    //quality should be in the range [0..100] .. 100 for max, 0 for min (0 best compression)
                    finalImage.Save(filePath, jpgInfo, encParams);
                    return true;
                }
            }
            catch { }
            if (bitmap != null)
            {
                bitmap.Dispose();
            }
            return false;
        }
    }
}