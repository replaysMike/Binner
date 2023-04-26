using System;

namespace Binner.Common.IO
{
    public static class FileUtils
    {
        /// <summary>
        /// Get the file size in a user friendly format.
        /// </summary>
        /// <example>1.9MB, 1KB</example>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static string GetFriendlyFileSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }
    }
}
