using System.IO;
using System.Reflection;

namespace Binner.Common.IO
{
    /// <summary>
    /// Resource loading library
    /// </summary>
    public static class ResourceLoader
    {
        /// <summary>
        /// Load an embedded resource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Binner.Common.{name}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
