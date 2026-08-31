namespace PriceDropApi.Models
{
    public class RegisterUserDto
    {
        public string Login { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
