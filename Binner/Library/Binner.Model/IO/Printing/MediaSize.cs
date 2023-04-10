namespace Binner.Model.IO.Printing
{
    public class MediaSize
    {
        /// <summary>
        /// The pixel width of the label image
        /// </summary>
        public int Width { get; set; } = 36;

        /// <summary>
        /// The pixel height of the label image
        /// </summary>
        public int Height { get; set; } = 136;

        /// <summary>
        /// The friendly name of the label
        /// </summary>
        public string Name { get; set; } = "1/2 in x 1-7/8 in";

        /// <summary>
        /// The media size driver name
        /// </summary>
        public string DriverName { get; set; } = "w36h136";

        /// <summary>
        /// The model name/number of the label
        /// </summary>
        public string ModelName { get; set; } = "30346";

        /// <summary>
        /// Additional descriptor information
        /// </summary>
        public string ExtraData { get; set; } = "";

        public MediaSize() { }

        /// <summary>
        /// Create a media size definition
        /// </summary>
        /// <param name="width">The pixel width of the label image</param>
        /// <param name="height">The pixel height of the label image</param>
        /// <param name="name">The friendly name of the label</param>
        /// <param name="driverName">The media size driver name of the label</param>
        /// <param name="modelName">The model name/number of the label</param>
        /// <param name="extraData"></param>
        public MediaSize(int width, int height, string name, string driverName, string modelName, string extraData = "")
        {
            Width = width;
            Height = height;
            Name = name;
            DriverName = driverName;
            ModelName = modelName;
            ExtraData = extraData;
        }
    }
}
