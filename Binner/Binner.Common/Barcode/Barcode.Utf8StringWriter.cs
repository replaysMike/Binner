using System.IO;
using System.Text;

namespace Binner.Common.Barcode
{

    public partial class Barcode
    {
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }
}