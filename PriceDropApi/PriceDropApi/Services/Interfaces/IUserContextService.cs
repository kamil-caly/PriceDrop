using System.Security.Claims;

namespace PriceDropApi.Services.Interfaces
{
    public interface IUserContextService
    {
        int? GetUserId();
        ClaimsPrincipal? User { get; }
    }
}
