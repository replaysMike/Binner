namespace Binner.Model.Integrations.Tme
{
    public static class TmeLanguages
    {
        public static List<string> Languages = new List<string>
        {
            "AT",
            "BG",
            "CH",
            "CZ",
            "DE",
            "EE",
            "EN",
            "ES",
            "FI",
            "FR",
            "GR",
            "HR",
            "HU",
            "IT",
            "LT",
            "LV",
            "NL",
            "PL",
            "PT",
            "RO",
            "RU",
            "SE",
            "SK",
            "TR",
            "UA"
        };

        /// <summary>
        /// Maps an official ISO2 language to a TME language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string MapLanguage(Languages language)
        {
            switch (language.ToString().ToUpper())
            {
                case "BG":
                    return "BG"; // Bulgarian
                case "RM":
                    return "CH"; // Switzerland, romansh
                case "CZ":
                    return "CZ"; // Czech
                case "DE":
                    return "AT"; // Austria / German
                case "ET":
                    return "ET"; // Estonia, Estonian
                case "ES":
                    return "ES"; // Spanish
                case "FI":
                    return "FI"; // Finnish
                case "FR":
                    return "FR"; // French
                case "EL":
                    return "GR"; // Greek
                case "HR":
                    return "HR"; // Croatian
                case "HU":
                    return "HU"; // Hungarian
                case "IT":
                    return "IT"; // Italian
                case "LT":
                    return "LT"; // Lithuanian
                case "LV":
                    return "LV"; // Latvian
                case "NL":
                    return "NL"; // Dutch
                case "PL":
                    return "PL"; // Polish
                case "PT":
                    return "PT"; // Portuguese
                case "RO":
                    return "RO"; // Romanian
                case "RU":
                    return "RU"; // Russian
                case "SE":
                    return "SV";  // Swedish
                case "SK":
                    return "SK";  // Solvak
                case "TR":
                    return "TR"; // Turkish
                case "UA":
                    return "UK";  // Ukrainian
                case "EN":
                default:
                    return "EN";
            }
        }
    }
}
