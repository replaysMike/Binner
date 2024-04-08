namespace Binner.Model.Integrations.Tme
{
    public class ProductList
    {
        public string Symbol { get; set; } = string.Empty;
        public TmeFile Files { get; set; } = new();
    }

    public class TmeFile
    {
        public List<string> PhotoList { get; set; } = new();
        public List<string> ThumbnailList { get; set; } = new();

        public List<string> HighResolutionPhotoList { get; set; } = new();
        public List<TmePresentation> PresentationList { get; set; } = new();
        public List<TmePhoto> AdditionalPhotoList { get; set; } = new();
        public List<TmeDocument> DocumentList { get; set; } = new();
        public List<TmeParametersImage> ParametersImages { get; set; } = new();
    }

    public class TmePhoto
    {
        public string Photo { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string HighResolutionPhoto { get; set; } = string.Empty;
    }

    public class TmePresentation
    {
    }

    public class TmeDocument
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public DocumentTypes DocumentType { get; set; }
        public int Filesize { get; set; }
        public string Language { get; set; } = string.Empty;
    }

    public class TmeParametersImage
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    // not part of the api. used to create a single collection of photos and their different resolutions
    public class TmePhotoFile
    {
        public string Photo { get; set; } = string.Empty;
        public TmePhotoResolution Resolution { get; set; }
        public TmePhotoFile(string photo, TmePhotoResolution resolution)
        {
            Photo = photo;
            Resolution = resolution;
        }
    }

    public enum TmePhotoResolution
    {
        Default,
        High,
        Thumbnail
    }
}
