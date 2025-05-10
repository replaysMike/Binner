namespace Binner.Model.KiCad.Responses
{
    public class KiCadCategoriesResponse
    {
        public ICollection<KiCadCategory> Categories { get; set; } = new List<KiCadCategory>();
    }
}
