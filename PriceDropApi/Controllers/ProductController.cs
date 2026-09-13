using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceDropApi.Models;
using PriceDropApi.Models.Enums;
using PriceDropApi.Services.Interfaces;
using PriceDropApi.Services.Interfaces.Shops;

namespace PriceDropApi.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IXKomService xKomService;
        private readonly IMoreleService moreleService;
        private readonly IProductService productService;

        public ProductController(IXKomService xKomService, IMoreleService moreleService, IProductService productService)
        {
            this.xKomService = xKomService;
            this.moreleService = moreleService;
            this.productService = productService;
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

        [Authorize]
        [HttpGet("get-products")]
        public async Task<ActionResult> GetProducts()
        {
            return Ok(await productService.GetProductsAsync());
        }
    }
}
