using Force.Crc32;
using System.IO;

namespace Binner.Common.IO
{
    public static class Checksum
    {
        /// <summary>
        /// Compute a checksum on a series of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int Compute(byte[] bytes)
        {
            // computes CRC-32C (Castagnoli) checksum, Intel's CRC32 instruction hardware acceleration
            var crc32 = Crc32CAlgorithm.Compute(bytes, 0, bytes.Length);
            return (int)crc32;
        }

        /// <summary>
        /// Compute a checksum on a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int Compute(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();
            return Compute(bytes);
        }
    }
}
