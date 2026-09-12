namespace PriceDropApi.Services.Interfaces.Shops
{
    public interface IMoreleService
    {
        Task<decimal> GetPrice(string productUrl);
    }
}
