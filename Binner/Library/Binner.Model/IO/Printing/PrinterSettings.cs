using Binner.Model.Configuration;

namespace Binner.Model.IO.Printing
{
    public class PrinterSettings : IPrinterSettings
    {
        /// <summary>
        /// Choose the print mode to use for printing labels.
        /// </summary>
        public PrintModes PrintMode { get; set; } = PrintModes.Direct;

        public string PrinterName { get; set; } = "Dymo LabelWriter 450 Twin Turbo";
        
        public string PartLabelName { get; set; } = "30346"; // LW 1/2" x 1 7/8"
        
        public LabelSource PartLabelSource { get; set; } = LabelSource.Auto;

        /// <summary>
        /// List of label definitions
        /// </summary>
        [Obsolete]
        public IEnumerable<LabelDefinition> LabelDefinitions { get; set; } = new List<LabelDefinition>()
        {
            new LabelDefinition()
            {
                MediaSize = new MediaSize(82, 248, "File Folder (2 up)", "w82h248", "30277", ""),
                LabelCount = 2,
                TotalLines = 2,
                TopMargin = -20,
                LeftMargin = 0,
                HorizontalDpi = 600,
                Dpi = 300
            },
            new LabelDefinition()
            {
                MediaSize = new MediaSize(36, 136, "1/2 in x 1-7/8 in", "w36h136", "30346", ""),
                LabelCount = 1,
                TotalLines = 2,
                TopMargin = -20,
                LeftMargin = 0,
                HorizontalDpi = 600,
                Dpi = 300
            },
            new LabelDefinition()
            {
                MediaSize = new MediaSize(79, 252, "Address", "w79h252", "30252", ""),
                LabelCount = 1,
                TotalLines = 4,
                TopMargin = -20,
                LeftMargin = 0,
                HorizontalDpi = 600,
                Dpi = 300
            },
            new LabelDefinition()
            {
                MediaSize = new MediaSize(57, 286, "File Folder", "w57h248", "30327", ""),
                LabelCount = 1,
                TotalLines = 4,
                TopMargin = -20,
                LeftMargin = 0,
                HorizontalDpi = 600,
                Dpi = 300
            }
        };

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        public PartLabelTemplate PartLabelTemplate { get; set; } = new PartLabelTemplate();
    }
}
