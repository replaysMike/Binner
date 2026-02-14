using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public interface IGlobalData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        Guid GlobalId { get; set; }
    }
}
