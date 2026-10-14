using System.Web;
using System.Web.Http;
using LegacyInventory.Core.Services;
using log4net;

namespace LegacyInventory.Api.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductsController));

        private readonly InventoryService _service = new InventoryService();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            return Ok(_service.GetProducts());
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var product = _service.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        [Route("{id:int}/adjust")]
        public IHttpActionResult Adjust(int id, [FromBody] StockAdjustment adjustment)
        {
            if (adjustment == null)
            {
                return BadRequest("Adjustment body is required.");
            }

            var clientIp = HttpContext.Current.Request.UserHostAddress;
            Log.InfoFormat("Stock adjustment for {0} by {1} from {2}", id, adjustment.Delta, clientIp);

            if (!_service.AdjustStock(id, adjustment.Delta))
            {
                return BadRequest("Product not found or insufficient stock.");
            }

            return Ok();
        }
    }

    public class StockAdjustment
    {
        public int Delta { get; set; }
    }
}
