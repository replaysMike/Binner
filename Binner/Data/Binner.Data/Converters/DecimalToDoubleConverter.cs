using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Binner.Data.Converters
{
    public class DecimalToDoubleConverter : ValueConverter<decimal, double>
    {
        public DecimalToDoubleConverter() : base(v => Convert.ToDouble(v), v => Convert.ToDecimal(v))
        {
        }
    }
}
