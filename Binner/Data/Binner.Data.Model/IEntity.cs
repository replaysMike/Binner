using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public interface IEntity
    {
        /// <summary>
        /// Creation date
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modified date
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        DateTime DateModifiedUtc { get; set; }
    }
}
