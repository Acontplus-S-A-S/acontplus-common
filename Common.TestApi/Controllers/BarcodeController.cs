using Common.Barcode.Models;
using Common.Barcode.Utils;

namespace Common.TestApi.Controllers;

public class BarcodeController : BaseApiController
{
    [HttpGet]
    public IActionResult Get(string text, bool includeLabel = false)
    {
        var barcodeConfig = new BarcodeConfig
        {
            Text = text ?? "0605202201030150819800120010030000012904948150712",
            Format = ZXing.BarcodeFormat.QR_CODE,
            IncludeLabel = includeLabel
        };
        var barcode = BarcodeGen.Create(barcodeConfig);
        return File(barcode, "image/png", "ci.png");
    }
}
