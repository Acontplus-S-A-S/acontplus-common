using Common.Barcode.Models;
using Common.Barcode.Utils;

namespace Common.TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var barcodeConfig = new BarcodeConfig
            {
                Text = "0605202201030150819800120010030000012904948150712",
                Format = ZXing.BarcodeFormat.QR_CODE
            };
            var barcode = BarcodeGen.Create(barcodeConfig);
            return File(barcode, "image/png", "ci.png");
        }
    }
}
