using Codeuctivity.ImageSharpCompare;
using SixLabors.ImageSharp;

namespace Binner.Testing
{
    public static class ImageValidator
    {
        /// <summary>
        /// Assert that images are exactly pixel equal
        /// </summary>
        /// <param name="path"></param>
        /// <param name="generatedImage"></param>
        /// <returns></returns>
        public static bool AssertValidateImageEquality(string path, MemoryStream generatedImage)
        {
            using var referenceImage = new MemoryStream();
            var referenceBytes = File.ReadAllBytes(path);
            referenceImage.Write(referenceBytes, 0, referenceBytes.Length);
            var areEqual = AreEqual(referenceImage, generatedImage);
            Assert.That(areEqual, Is.True);
            return areEqual;
        }

        /// <summary>
        /// Assert that images are exactly pixel equal
        /// </summary>
        /// <param name="path"></param>
        /// <param name="generatedImage"></param>
        /// <returns></returns>
        public static bool AssertValidateImageEquality(string path, Image generatedImage)
        {
            var referenceImage = Image.Load(path);
            var areEqual = AreEqual(referenceImage, generatedImage);
            Assert.That(areEqual, Is.True);
            return areEqual;
        }

        public static bool AreEqual(MemoryStream referenceStream, MemoryStream generatedStream)
        {
            referenceStream.Position = 0;
            generatedStream.Position = 0;
            return ImageSharpCompare.ImagesAreEqual(referenceStream, generatedStream);
        }

        public static bool AreEqual(Image referenceImage, Image generatedImage)
        {
            return ImageSharpCompare.ImagesAreEqual(referenceImage, generatedImage);
        }

        public static bool AreSimilar(MemoryStream referenceStream, MemoryStream generatedStream, double errorPercentage)
        {
            referenceStream.Position = 0;
            generatedStream.Position = 0;
            var result = ImageSharpCompare.CalcDiff(referenceStream, generatedStream);
            if (result.PixelErrorPercentage < errorPercentage)
                return true;
            return false;
        }

        public static bool AreSimilar(Image referenceImage, Image generatedImage, double errorPercentage)
        {
            var result = ImageSharpCompare.CalcDiff(referenceImage, generatedImage);
            if (result.PixelErrorPercentage < errorPercentage)
                return true;
            return false;
        }
    }
}
