using Binner.Model.Swarm;

namespace Binner.Model.Requests
{
    public class UploadPartNumberManufacturerFilesRequest<T>
    {
        /// <summary>
        /// Part id associated with the file
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// Stored file type
        /// </summary>
        public ImageTypes ImageType { get; set; } = ImageTypes.ProductShot;

        /// <summary>
        /// List of uploaded files
        /// </summary>
        public ICollection<T>? Files { get; set; }
    }
}
