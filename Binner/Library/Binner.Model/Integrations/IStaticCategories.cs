namespace Binner.Model.Integrations
{
    public interface IStaticCategories
    {
        /// <summary>
        /// Maps a supplier category to a Part Type Id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        int? MatchToPartTypeId(int categoryId);

        /// <summary>
        /// Maps a supplier category name to a Part Type Id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int? MatchToPartTypeId(string name);

        /// <summary>
        /// Validate the category configuration
        /// </summary>
        /// <returns></returns>
        void Validate();
    }
}
