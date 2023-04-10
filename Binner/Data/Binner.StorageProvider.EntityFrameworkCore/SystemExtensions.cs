namespace Binner.StorageProvider.EntityFrameworkCore
{
    internal static class SystemExtensions
    {
        /// <summary>
        /// Uppercase the first character of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string UcFirst(this string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }
    }
}
