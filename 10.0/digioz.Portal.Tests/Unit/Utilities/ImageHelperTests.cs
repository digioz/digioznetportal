using System;
using System.IO;
using digioz.Portal.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SkiaSharp;

namespace digioz.Portal.Tests.Unit.Utilities
{
    [TestFixture]
    [Category("Unit")]
    [Category("Utilities")]
    public class ImageHelperTests
    {
        private string _tempDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"digioz-image-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        [Test]
        public void SaveJigsawImage_WithLandscapeImage_PreservesAspectRatio()
        {
            var input = CreateSolidImage(400, 200, SKColors.CornflowerBlue, SKEncodedImageFormat.Png);
            var outputPath = GetOutputPath("landscape.jpg");

            var result = ImageHelper.SaveJigsawImage(input, 100, 100, outputPath);

            result.Should().BeTrue();
            using var output = SKBitmap.Decode(outputPath);
            output.Should().NotBeNull();
            output.Width.Should().Be(100);
            output.Height.Should().Be(50);
        }

        [Test]
        public void SaveJigsawImage_WithPortraitImage_PreservesAspectRatio()
        {
            var input = CreateSolidImage(200, 400, SKColors.CornflowerBlue, SKEncodedImageFormat.Png);
            var outputPath = GetOutputPath("portrait.jpg");

            var result = ImageHelper.SaveJigsawImage(input, 100, 100, outputPath);

            result.Should().BeTrue();
            using var output = SKBitmap.Decode(outputPath);
            output.Should().NotBeNull();
            output.Width.Should().Be(50);
            output.Height.Should().Be(100);
        }

        [Test]
        public void SaveImageWithCrop_WithLandscapeImage_UsesCenteredSourceRegion()
        {
            var input = CreateStripedImage();
            var outputPath = GetOutputPath("crop.jpg");

            var result = ImageHelper.SaveImageWithCrop(input, 100, 100, outputPath);

            result.Should().BeTrue();
            using var output = SKBitmap.Decode(outputPath);
            output.Should().NotBeNull();
            output.Width.Should().Be(100);
            output.Height.Should().Be(100);
            var center = output.GetPixel(50, 50);
            center.Green.Should().BeGreaterThan(100);
            center.Red.Should().BeLessThan(80);
            center.Blue.Should().BeLessThan(80);
        }

        [Test]
        public void SaveImageWithCrop_WithValidInput_WritesJpegThatRoundTrips()
        {
            var input = CreateSolidImage(240, 160, SKColors.Orange, SKEncodedImageFormat.Png);
            var outputPath = GetOutputPath("round-trip.dat");

            var result = ImageHelper.SaveImageWithCrop(input, 120, 120, outputPath);

            result.Should().BeTrue();
            var outputBytes = File.ReadAllBytes(outputPath);
            outputBytes.Should().StartWith(new byte[] { 0xFF, 0xD8 });
            using var output = SKBitmap.Decode(outputBytes);
            output.Should().NotBeNull();
            output.Width.Should().Be(120);
            output.Height.Should().Be(120);
        }

        [Test]
        public void SaveImageWithResize_WithPngInput_PreservesEncodedFormat()
        {
            var input = CreateSolidImage(320, 160, SKColors.Purple, SKEncodedImageFormat.Png);
            var outputPath = GetOutputPath("thumbnail.png");

            var result = ImageHelper.SaveImageWithResize(input, 80, 80, outputPath);

            result.Should().BeTrue();
            using var outputData = SKData.Create(outputPath);
            using var codec = SKCodec.Create(outputData);
            codec.Should().NotBeNull();
            codec.EncodedFormat.Should().Be(SKEncodedImageFormat.Png);
            codec.Info.Width.Should().Be(80);
            codec.Info.Height.Should().Be(40);
        }

        [Test]
        public void SaveImageWithCrop_WithInvalidInput_ReturnsFalseWithoutOutput()
        {
            var outputPath = GetOutputPath("invalid.jpg");

            var result = ImageHelper.SaveImageWithCrop(new byte[] { 1, 2, 3, 4 }, 100, 100, outputPath);

            result.Should().BeFalse();
            File.Exists(outputPath).Should().BeFalse();
        }

        private string GetOutputPath(string fileName)
        {
            return Path.Combine(_tempDirectory, fileName);
        }

        private static byte[] CreateSolidImage(
            int width,
            int height,
            SKColor color,
            SKEncodedImageFormat format)
        {
            using var bitmap = new SKBitmap(width, height);
            bitmap.Erase(color);
            return Encode(bitmap, format);
        }

        private static byte[] CreateStripedImage()
        {
            using var bitmap = new SKBitmap(300, 100);
            using var canvas = new SKCanvas(bitmap);
            using var paint = new SKPaint();

            paint.Color = SKColors.Red;
            canvas.DrawRect(0, 0, 100, 100, paint);
            paint.Color = SKColors.Green;
            canvas.DrawRect(100, 0, 100, 100, paint);
            paint.Color = SKColors.Blue;
            canvas.DrawRect(200, 0, 100, 100, paint);
            canvas.Flush();

            return Encode(bitmap, SKEncodedImageFormat.Png);
        }

        private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(format, 100);
            return data.ToArray();
        }
    }
}
