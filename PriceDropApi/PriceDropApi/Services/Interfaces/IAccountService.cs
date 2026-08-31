using PriceDropApi.Models;

namespace PriceDropApi.Services.Interfaces
{
    public interface IAccountService
    {
        void RegisterUser(RegisterUserDto dto);
    }
}
