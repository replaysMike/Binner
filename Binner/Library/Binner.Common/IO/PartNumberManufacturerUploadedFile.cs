using Binner.Model;
using Binner.Model.Swarm;
using System.IO;

namespace Binner.Common.IO
{
    public class PartNumberManufacturerUploadedFile : UploadedFile
    {
        /// <summary>
        /// The associated part number id
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        public ImageTypes ImageType { get; set; }

        public PartNumberManufacturerUploadedFile(string filename, Stream stream, int partNumberManufacturerId, ImageTypes imageType)
            : base(filename, stream)
        {
            PartNumberManufacturerId = partNumberManufacturerId;
            ImageType = imageType;
        }
    }
}
