using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceDropApi.Models;
using PriceDropApi.Models.Enums;
using PriceDropApi.Services.Interfaces;
using PriceDropApi.Services.Interfaces.Shops;

namespace PriceDropApi.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IXKomService xKomService;
        private readonly IMoreleService moreleService;

        public ProductController(IXKomService xKomService, IMoreleService moreleService)
        {
            this.xKomService = xKomService;
            this.moreleService = moreleService;
        }

        [HttpGet("get-price")]
        public async Task<ActionResult> GetPrice([FromQuery] GetPriceDto dto)
        {
            return dto.ShopType switch
            {
                ShopType.XKom => Ok(await xKomService.GetPrice(dto.ProductUrl)),
                ShopType.Morele => Ok(await moreleService.GetPrice(dto.ProductUrl)),
                _ => BadRequest("Unsupported shop type.")
            };
        }
    }
}
