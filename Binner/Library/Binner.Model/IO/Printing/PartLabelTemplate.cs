namespace Binner.Model.IO.Printing
{
    public class PartLabelTemplate
    {
        public LineConfiguration Line1 { get; set; } = new LineConfiguration();
        public LineConfiguration Line2 { get; set; } = new LineConfiguration();
        public LineConfiguration Line3 { get; set; } = new LineConfiguration();
        public LineConfiguration Line4 { get; set; } = new LineConfiguration();
        /// <summary>
        /// Optional Location identifier
        /// </summary>
        public LineConfiguration Identifier { get; set; } = new LineConfiguration();
        /// <summary>
        /// Optional Location identifier 2
        /// </summary>
        public LineConfiguration Identifier2 { get; set; } = new LineConfiguration();
    }
}
