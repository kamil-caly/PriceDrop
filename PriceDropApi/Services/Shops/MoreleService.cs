using HtmlAgilityPack;
using PriceDropApi.Services.Interfaces.Shops;
using System.Text.RegularExpressions;

namespace PriceDropApi.Services.Shops
{
    public class MoreleService : IMoreleService
    {
        public async Task<decimal> GetPrice(string productUrl)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(productUrl);

            string priceString = doc.DocumentNode
                .SelectSingleNode("//*[@id='product_price']")
                ?.InnerText
                ?? "0";

            priceString = Regex.Replace(priceString, @"[^0-9,]", "");

            return decimal.Parse(priceString);
        }
    }
}
