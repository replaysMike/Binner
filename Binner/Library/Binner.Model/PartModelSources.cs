namespace Binner.Model
{
    /// <summary>
    /// The type of part model
    /// </summary>
    public enum PartModelSources
    {
        UserUpload = 0,
        Binner,
        // https://ultralibrarian.com
        UltraLibrarian,
        // https://snapmagic.com, https://snapeda.com
        SnapMagic,
        // https://componentsearchengine.com
        ComponentSearchEngine,
        // https://3dfindit.com
        ThreeDFindIt,
        Other
    }
}
