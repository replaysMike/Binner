namespace Binner.Model
{
    public enum CustomFieldTypes
    {
        /// <summary>
        /// Custom field associated with parts
        /// </summary>
        Inventory = 0,
        /// <summary>
        /// Custom field associated with BOM projects
        /// </summary>
        Project,
        /// <summary>
        /// Custom field associated with part types
        /// </summary>
        PartType,
        /// <summary>
        /// Custom field associated with printing labels
        /// </summary>
        Label,
        /// <summary>
        /// Custom field associated with part suppliers
        /// </summary>
        PartSupplier,
        /// <summary>
        /// Custom field associated with PCBs
        /// </summary>
        Pcb,
        /// <summary>
        /// Custom field associated with users
        /// </summary>
        User
    }
}
