﻿using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class PartParametric : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartParametricId { get; set; }

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Name of parametric
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Value of parametric
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Value as a number if numeric
        /// </summary>
        public double ValueNumber { get; set; }

        /// <summary>
        /// The measurement units of the value
        /// </summary>
        public ParametricUnits Units { get; set; }

        public int DigiKeyParameterId { get; set; }

        public int DigiKeyValueId { get; set; }

        public string? DigiKeyParameterType { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

#if INITIALCREATE
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        public Part? Part { get; set; }
    }
}
