using Binner.Common.Models;

namespace Binner.Common.Configuration
{
    public class LocaleConfiguration
    {
        public Languages Language { get; set; } = Languages.En;

        /// <summary>
        /// Valid currency values: USD, CAD, JPY, GBP, EUR, HKD, SGD, TWD, KRW, AUD, NZD, INR, DKK, NOK, SEK, ILS, CNY, PLN, CHF, CZK, HUF, RON, ZAR, MYR, THB, PHP
        /// </summary>
        public Currencies Currency { get; set; } = Currencies.USD;
    }
}
