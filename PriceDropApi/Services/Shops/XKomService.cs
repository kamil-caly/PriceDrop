using HtmlAgilityPack;
using PriceDropApi.Services.Interfaces.Shops;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PriceDropApi.Services.Shops
{
    public class XKomService : IXKomService
    {
        public async Task<decimal> GetPrice(string productUrl)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(productUrl);

            string priceString = doc.DocumentNode
                .SelectSingleNode("//*[@data-name='buybox']//*[@data-name='productPrice']/span")
                ?.InnerText
                ?? "0";

            priceString = Regex.Replace(priceString, @"[^0-9,]", "");

            return decimal.Parse(priceString, CultureInfo.GetCultureInfo("pl-PL"));
        }
    }
}
