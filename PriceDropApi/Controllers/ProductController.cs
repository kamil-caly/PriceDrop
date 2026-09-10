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
        private readonly IMediaExpertService mediaExpertService;

        public ProductController(IMediaExpertService mediaExpertService)
        {
            this.mediaExpertService = mediaExpertService;
        }

        [HttpGet("get-price")]
        public ActionResult GetPrice([FromQuery] GetPriceDto dto)
        {
            return dto.ShopType switch
            {
                ShopType.MediaExpert => Ok(mediaExpertService.GetPrice(dto.ShopUrl)),
                _ => BadRequest("Unsupported shop type.")
            };
        }
    }
}
