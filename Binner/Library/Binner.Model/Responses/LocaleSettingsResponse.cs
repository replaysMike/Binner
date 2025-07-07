namespace Binner.Model.Responses
{
    public class LocaleSettingsResponse
    {
        /// <summary>
        /// Default language
        /// </summary>
        public Languages Language { get; set; } = Languages.En;

        /// <summary>
        /// Default currency
        /// </summary>
        public Currencies Currency { get; set; } = Currencies.USD;
    }
}
