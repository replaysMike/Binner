namespace Binner.Model.Configuration
{
    public class LocaleConfiguration
    {
        private Languages _language = Languages.En;
        /// <summary>
        /// Default language to be used by the API.
        /// Valid values: en, br, cs, da, de, es, fi, fr, he, hu, it, ja, ko, nl, no, pl, pt, ro, sv, th, zhs, zht, bg, rm, el, hr, lt, lv, ru, sk, tr, uk
        /// </summary>
        public Languages Language
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Language)))
                    return Enum.Parse<Languages>(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Language) ?? "USD");
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        private Currencies _currency = Currencies.USD;
        /// <summary>
        /// Default currency to be used by the API.
        /// Valid currency values: USD, CAD, JPY, GBP, EUR, HKD, SGD, TWD, KRW, AUD, NZD, INR, DKK, NOK, SEK, ILS, CNY, PLN, CHF, CZK, HUF, RON, ZAR, MYR, THB, PHP
        /// </summary>
        public Currencies Currency
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Currency)))
                    return Enum.Parse<Currencies>(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Currency) ?? "USD");
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }
    }
}
