using System.IO;

namespace Binner.Common.IO
{
    public class CircuitUploadedFile : UploadedFile
    {
        /// <summary>
        /// The associated part number id
        /// </summary>
        public int? CircuitId { get; set; }

        public CircuitUploadedFile(string filename, Stream stream, int? circuitId)
            : base(filename, stream)
        {
            CircuitId = circuitId;
        }
    }
}
