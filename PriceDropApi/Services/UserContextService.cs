using PriceDropApi.Services.Interfaces;
using System.Security.Claims;

namespace PriceDropApi.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;
        public int? GetUserId()
        {
            if (User is null) return null;
            string? userIdClaim = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        public string? GetUserLogin()
        {
            if (User is null) return null;
            return User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
        }
    }
}
