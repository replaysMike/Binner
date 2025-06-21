using Binner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Services.Integrations.Barcode
{
    public interface IBarcodeInfoService
    {
        Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType);
    }
}
