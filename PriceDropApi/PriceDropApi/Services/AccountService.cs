using Microsoft.AspNetCore.Identity;
using PriceDropApi.Entities;
using PriceDropApi.Models;
using PriceDropApi.Services.Interfaces;

namespace PriceDropApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly PriceDropDbContext dbCtx;
        private readonly IPasswordHasher<User> passwordHasher;

        public AccountService(PriceDropDbContext dbCtx, IPasswordHasher<User> passwordHasher)
        {
            this.dbCtx = dbCtx;
            this.passwordHasher = passwordHasher;
        }

        public void RegisterUser(RegisterUserDto dto)
        {
            var newUser = new User()
            {
                Login = dto.Login,
                CreatedAt = DateTime.Now
            };

            var hashedPassword = passwordHasher.HashPassword(newUser, dto.Password);
            newUser.PasswordHash = hashedPassword;
            dbCtx.Users.Add(newUser);
            dbCtx.SaveChanges();
        }
    }
}
