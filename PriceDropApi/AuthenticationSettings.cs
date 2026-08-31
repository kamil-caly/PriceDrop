namespace PriceDropApi
{
    public class AuthenticationSettings
    {
        public string JwtKey { get; set; } = default!;
        public string JwtExpireDays { get; set; } = default!;
        public string JwtIssuer { get; set; } = default!;
    }
}
