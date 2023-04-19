using System.Globalization;

namespace Binner.Model.Integrations.Mouser
{
    public class MouserPart
    {
        public string? Availability { get; set; }
        public int AvailabilityInteger
        {
            get
            {
                var partCount = 0;
                if (!string.IsNullOrEmpty(Availability))
                {
                    var parts = Availability.Split(' ');
                    if (parts.Length > 0)
                        int.TryParse(parts[0], out partCount);
                }
                return partCount;
            }
        }
        public string? DataSheetUrl { get; set; }
        public string? Description { get; set; }
        public string? FactoryStock { get; set; }
        public string? ImagePath { get; set; }
        public string? Category { get; set; }
        public string? LeadTime { get; set; }
        public string? LifecycleStatus { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? Min { get; set; }
        public string? Mult { get; set; }
        public string? MouserPartNumber { get; set; }
        public ICollection<ProductAttribute>? ProductAttributes { get; set; }
        public ICollection<PriceBreak>? PriceBreaks { get; set; }
        public ICollection<AlternatePackaging>? AlternatePackagings { get; set; }
        public string? ProductDetailUrl { get; set; }
        public bool Reeling { get; set; }
        public string? ROHSStatus { get; set; }
        public int MultiSimBlue { get; set; }
        public UnitWeightKg? UnitWeightKg { get; set; }
        public string? RestrictionMessage { get; set; }
        public string? PID { get; set; }
        public ICollection<ProductCompliance>? ProductCompliance { get; set; }
    }

    public class ProductAttribute
    {
        public string? AttributeName { get; set; }
        public string? AttributeValue { get; set; }
    }

    public class PriceBreak
    {
        public long Quantity { get; set; }
        public string? Price { get; set; }
        public string? Currency { get; set; }
        public double Cost => FromCurrency(Price);

        /// <summary>
        /// Get the numeric value from a currency formatted string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public double FromCurrency(string? input)
        {
            if (input == null) return 0d;

            var result = 0d;
            var allCulturesByCurrencySymbol = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .GroupBy(c => c.NumberFormat.CurrencySymbol)
                .ToDictionary(c => c.Key, c => c.ToList());
            var culturesMatchingInputCurrencySymbol = allCulturesByCurrencySymbol.FirstOrDefault(c => input.Contains(c.Key));

            // try to find the closest matching culture to the input string
            var foundMatching = false;
            if (culturesMatchingInputCurrencySymbol.Value?.Any() == true)
            {
                foreach (var c in culturesMatchingInputCurrencySymbol.Value)
                {
                    if (double.TryParse(input, NumberStyles.Currency | NumberStyles.AllowDecimalPoint, c, out var successResult))
                    {
                        // success, use this culture
                        foundMatching = true;
                        result = successResult;
                        break;
                    }
                }
            }

            // found no matching value to the format
            if (!foundMatching)
            {
                if (!double.TryParse(input, NumberStyles.Currency | NumberStyles.AllowDecimalPoint, System.Threading.Thread.CurrentThread.CurrentCulture, out result))
                    double.TryParse(input, out result);
            }

            return result;
        }
    }

    public class AlternatePackaging
    {
        public string? APMfrPN { get; set; }
    }

    public class UnitWeightKg
    {
        public double UnitWeight { get; set; }
    }

    public class ProductCompliance
    {
        public string? ComplianceName { get; set; }
        public string? ComplianceValue { get; set; }
    }
}
