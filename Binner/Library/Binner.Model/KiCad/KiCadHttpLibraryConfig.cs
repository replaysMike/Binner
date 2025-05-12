using Newtonsoft.Json;

namespace Binner.Model.KiCad
{
    public class KiCadHttpLibraryConfig
    {
        public MetaObject Meta { get; set; } = new();
        public string Name { get; set; } = "Binner HTTP Library";
        public string Description { get; set; } = "A connected library of part information.";
        public Source Source { get; set; } = new();

        public KiCadHttpLibraryConfig() { }
        
        public KiCadHttpLibraryConfig(string token)
        {
            Source.Token = token;
        }

        public KiCadHttpLibraryConfig(string token, KiCadTimeouts? timeouts) : this(token)
        {
            if (timeouts != null)
            {
                Source.TimeOutCategoriesSeconds = timeouts.TimeOutCategoriesSeconds;
                Source.TimeOutPartsSeconds = timeouts.TimeOutPartsSeconds;
            }
        }
    }

    public class KiCadTimeouts
    {
        [JsonProperty("timeout_parts_seconds")]
        public int TimeOutPartsSeconds { get; set; } = 60;

        [JsonProperty("timeout_categories_seconds")]
        public int TimeOutCategoriesSeconds { get; set; } = 600;
    }

    public class Source : KiCadTimeouts
    {
        public string Type { get; set; } = "REST_API";
        
        [JsonProperty("api_version")]
        public string Api_Version { get; set; } = "v1";

        [JsonProperty("root_url")]
        public string Root_Url { get; set; } = "https://localhost:8090/kicad-api";
        
        public string Token { get; set; } = string.Empty;
    }

    public class MetaObject
    {
        public double Version { get; set; } = 1.0;
    }
}
