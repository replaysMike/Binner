using shortid;
using shortid.Configuration;

namespace Binner.Common
{
    public static class ShortIdGenerator
    {
        private static readonly GenerationOptions DefaultOptions = new(true, true, 10);

        /// <summary>
        /// Generate a new short id using the default options (length = 10)
        /// </summary>
        /// <returns></returns>
        public static string Generate() => ShortId.Generate(DefaultOptions);

        /// <summary>
        /// Generate a new short id using the specified options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Generate(GenerationOptions options) => ShortId.Generate(options);

        /// <summary>
        /// Generate a new short id using a custom length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Generate(int length) => ShortId.Generate(new GenerationOptions(true, true, length));
    }
}
