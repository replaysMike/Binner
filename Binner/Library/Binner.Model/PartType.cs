﻿using System.ComponentModel.DataAnnotations;

namespace Binner.Model
{
    /// <summary>
    /// Defines a type of part or category/sub-category
    /// </summary>
    public class PartType : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public long PartTypeId { get; set; }

        /// <summary>
        /// If this is a child type, indicates the parent type
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        /// <summary>
        /// The name of the part type
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Part type description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Reference designator
        /// </summary>
        public string? ReferenceDesignator { get; set; }

        /// <summary>
        /// The symbol id of the part type (KiCad)
        /// </summary>
        public string? SymbolId { get; set; }

        /// <summary>
        /// Optional keywords to help with search
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Name or SVG content of icon
        /// If left empty, default icon choices will be applied.
        /// </summary>
        public string? Icon { get;set; }

        public ICollection<CustomValue> CustomFields { get; set; } = new List<CustomValue>();

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is PartType partType)
                return Equals(partType);
            return false;
        }

        public bool Equals(PartType other)
        {
            return other != null && PartTypeId == other.PartTypeId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return PartTypeId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(PartTypeId, UserId);
#endif

        }

        public override string ToString()
        {
            return $"{PartTypeId}: {Name}";
        }
    }
}
