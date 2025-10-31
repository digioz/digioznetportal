using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

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

                using (var image = Image.Load(isd.ImagePath))
                {
                    if (SaveJigsawImage(image, isd.FullMaxWidth, isd.FullMaxHeight, fullFilePath))
                    {
                        using (var smallImage = Image.Load(isd.ImagePath))
                        {
                            if (SaveJigsawImage(smallImage, isd.SmallMaxWidth, isd.SmallMaxHeight, smallFilePath))
                            {
                                using (var tinyImage = Image.Load(isd.ImagePath))
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
            catch (Exception)
            {
                // Log exception here if needed
            }
            return false;
        }

        public static bool SaveJigsawImage(Image image, int maxWidth, int maxHeight, string filePath)
        {
            try
            {
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    double divisor;
                    int newWidth, newHeight;

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

                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                var encoder = new JpegEncoder { Quality = 100 };
                image.Save(filePath, encoder);
                return true;
            }
            catch (Exception)
            {
                // Log exception here if needed
                return false;
            }
        }

        public static bool SaveImageWithCrop(Image image, int maxWidth, int maxHeight, string filePath)
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

                image.Mutate(x => x
                    .Crop(new Rectangle(left, top, srcWidth, srcHeight))
                    .Resize(maxWidth, maxHeight));

                var encoder = new JpegEncoder { Quality = 100 };
                image.Save(filePath, encoder);
                return true;
            }
            catch (Exception)
            {
                // Log exception here if needed
                return false;
            }
        }
    }
}