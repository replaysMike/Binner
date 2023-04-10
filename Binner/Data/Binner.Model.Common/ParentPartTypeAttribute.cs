using static Binner.Model.Common.SystemDefaults;

namespace Binner.Model.Common
{
    /// <summary>
    /// Indicates the parent of a part type
    /// </summary>
    public class ParentPartTypeAttribute : Attribute
    {
        /// <summary>
        /// Get the parent part type
        /// </summary>
        public DefaultPartTypes Parent { get; }

        /// <summary>
        /// Indicates the parent of a part type
        /// </summary>
        /// <param name="parent"></param>
        public ParentPartTypeAttribute(DefaultPartTypes parent)
        {
            Parent = parent;
        }
    }
}
