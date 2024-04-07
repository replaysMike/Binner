namespace Binner.Model.Configuration
{
    public class LocaleConfiguration
    {
        /// <summary>
        /// Default language to be used by the API.
        /// Valid values: en, br, cs, da, de, es, fi, fr, he, hu, it, ja, ko, nl, no, pl, pt, ro, sv, th, zhs, zht, bg, rm, el, hr, lt, lv, ru, sk, tr, uk
        /// </summary>
        public Languages Language { get; set; } = Languages.En;

        /// <summary>
        /// Default currency to be used by the API.
        /// Valid currency values: USD, CAD, JPY, GBP, EUR, HKD, SGD, TWD, KRW, AUD, NZD, INR, DKK, NOK, SEK, ILS, CNY, PLN, CHF, CZK, HUF, RON, ZAR, MYR, THB, PHP
        /// </summary>
        public Currencies Currency { get; set; } = Currencies.USD;
    }
}
