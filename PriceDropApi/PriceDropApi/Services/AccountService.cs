using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PriceDropApi.Entities;
using PriceDropApi.Models;
using PriceDropApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PriceDropApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly PriceDropDbContext dbCtx;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly AuthenticationSettings authenticationSettings;

        public AccountService(PriceDropDbContext dbCtx, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings)
        {
            this.dbCtx = dbCtx;
            this.passwordHasher = passwordHasher;
            this.authenticationSettings = authenticationSettings;
        }

        public void RegisterUser(RegisterUserDto dto)
        {
            if (dbCtx.Users.Any(u => u.Login == dto.Login))
            {
                throw new Exception("User with this login already exists.");
            }

            var newUser = new User()
            {
                Login = dto.Login,
                CreatedAt = DateTime.UtcNow
            };

            var hashedPassword = passwordHasher.HashPassword(newUser, dto.Password);
            newUser.PasswordHash = hashedPassword;
            dbCtx.Users.Add(newUser);
            dbCtx.SaveChanges();
        }

        public string GenerateJwt(LoginUserDto dto)
        {
            var user = dbCtx
                .Users
                .FirstOrDefault(u => u.Login == dto.Login);

            if (user is null)
                throw new Exception("Invalid username or password");

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid username or password");

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(authenticationSettings.JwtExpireDays));

            var token = new JwtSecurityToken(authenticationSettings.JwtIssuer,
                authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
