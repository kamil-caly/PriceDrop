using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceDropApi.Models;
using PriceDropApi.Models.Enums;

namespace PriceDropApi.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet("get-price")]
        public ActionResult GetPrice([FromBody] GetPriceDto dto)
        {

        }
    }
}
