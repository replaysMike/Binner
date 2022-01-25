using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace Binner.Common.Services
{
    public class SettingsService : ISettingsService
    {
        private const string BackupFilenameExtension = ".bak";

        public void SaveSettingsAs<T>(T instance, string sectionName, string filename, bool createBackup)
        {
            if (createBackup)
            {
                File.Copy(filename, $"{filename}_{DateTime.Now.Ticks}{BackupFilenameExtension}", true);
            }

            // serialize settings to disk
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
            var serializer = new JsonSerializer();

            JObject json;
            using (var textReader = new StreamReader(filename))
            {
                using var reader = new JsonTextReader(textReader);
                json = (JObject)serializer.Deserialize(reader);
                textReader.Close();
            }

            if (json.ContainsKey(sectionName))
                json[sectionName] = JToken.FromObject(instance);
            else
                throw new BinnerConfigurationException($"There is no section named '{sectionName}' in the configuration file '{filename}'!");

            var jsonOutput = JsonConvert.SerializeObject(json, serializerSettings);
            using var file = File.Open(filename, FileMode.Open, FileAccess.Write, FileShare.Read);
            var buffer = Encoding.Default.GetBytes(jsonOutput);
            file.Write(buffer, 0, buffer.Length);
            file.Close();
        }
    }
}
