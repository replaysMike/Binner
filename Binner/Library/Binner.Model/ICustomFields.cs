using Binner.Global.Common;

namespace Binner.Model
{
    public interface ICustomFields
    {
        ICollection<CustomValue> CustomFields { get; set; }
    }
}
