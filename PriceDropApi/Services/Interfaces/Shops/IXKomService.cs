namespace PriceDropApi.Services.Interfaces.Shops
{
    public interface IXKomService
    {
        Task<decimal> GetPrice(string productUrl);
    }
}
