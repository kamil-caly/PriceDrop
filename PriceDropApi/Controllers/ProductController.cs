using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceDropApi.Models;
using PriceDropApi.Services.Interfaces;

namespace PriceDropApi.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet("get-price")]
        public async Task<ActionResult> GetPrice([FromQuery] GetPriceDto dto)
        {
            var price = await productService.GetPriceAsync(dto);

            return price is null
                ? BadRequest("Unsupported shop type.")
                : Ok((decimal)price);
        }

        [Authorize]
        [HttpGet("get-products")]
        public async Task<ActionResult<IEnumerable<GetProductsDto>>> GetProducts()
        {
            return Ok(await productService.GetProductsAsync());
        }

        [Authorize]
        [HttpPost("add-product")]
        public async Task<ActionResult> AddProduct([FromBody] AddProductDto dto)
        {
            var productId = await productService.AddProductAsync(dto);
            return Created($"product id: {productId}", null);
        }

        [Authorize]
        [HttpDelete("delete-product/{productId}")]
        public async Task<ActionResult> DeleteProduct([FromRoute] int productId)
        {
            await productService.DeleteProductAsync(productId);
            return NoContent();
        }
    }
}
