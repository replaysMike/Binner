using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
#if INITIALCREATE
    /// <summary>
    /// Stores user defined printer configurations
    /// </summary>
    public class UserLocaleConfiguration : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserLocaleConfigurationId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Default language to be used by the API.
        /// Valid values: en, br, cs, da, de, es, fi, fr, he, hu, it, ja, ko, nl, no, pl, pt, ro, sv, th, zhs, zht, bg, rm, el, hr, lt, lv, ru, sk, tr, uk
        /// </summary>
        public Languages Language { get; set; }

        /// <summary>
        /// Default currency to be used by the API.
        /// Valid currency values: USD, CAD, JPY, GBP, EUR, HKD, SGD, TWD, KRW, AUD, NZD, INR, DKK, NOK, SEK, ILS, CNY, PLN, CHF, CZK, HUF, RON, ZAR, MYR, THB, PHP
        /// </summary>
        public Currencies Currency { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
#endif
}
