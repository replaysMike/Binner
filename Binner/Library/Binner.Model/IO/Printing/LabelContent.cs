namespace Binner.Model.IO.Printing
{
    public class LabelContent
    {
        /// <summary>
        /// A part to use as the template source. If not provided, strings will be literals.
        /// </summary>
        public Part Part { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Identifier { get; set; }
        public string Identifier2 { get; set; }
    }
}
