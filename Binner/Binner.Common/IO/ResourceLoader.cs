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
        /// Load an embedded resource as a string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string LoadResourceString(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceName(assembly, name);

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Load an embedded resource stream
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Stream LoadResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceName(assembly, name);
            return assembly.GetManifestResourceStream(resourceName);
        }

        private static string GetResourceName(Assembly assembly, string resourceName) => $"{assembly.GetName().Name}.{resourceName}";
    }
}
