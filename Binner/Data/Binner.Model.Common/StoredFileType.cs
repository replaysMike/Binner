namespace Binner.Model.Common
{
    /// <summary>
    /// The type of stored file
    /// </summary>
    public enum StoredFileType
    {
        /// <summary>
        /// File is a product image
        /// </summary>
        ProductImage,
        /// <summary>
        /// File is a datasheet
        /// </summary>
        Datasheet,
        /// <summary>
        /// File is a pinout
        /// </summary>
        Pinout,
        /// <summary>
        /// File is a reference design
        /// </summary>
        ReferenceDesign,
        /// <summary>
        /// File is a product sheet
        /// </summary>
        ProductSheet,
        /// <summary>
        /// File is a marketing guide
        /// </summary>
        MarketingGuide,
        /// <summary>
        /// Other file type
        /// </summary>
        Other,
        /// <summary>
        /// Pcb layout files
        /// </summary>
        Pcb
    }
}
