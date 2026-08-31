using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceDropApi.Models;
using PriceDropApi.Services.Interfaces;
using System.Security.Claims;

namespace PriceDropApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
    
        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("register")]
        public ActionResult RegisterUser([FromBody] RegisterUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) 
                || string.IsNullOrWhiteSpace(dto.Password) 
                || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            {
                return BadRequest("Login, password and confirm password are required.");
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest("Password and confirm password do not match.");
            }

            accountService.RegisterUser(dto);
            return Ok();
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Login and password are required.");
            }

            string token = accountService.GenerateJwt(dto);
            return Ok(token);
        }

        [Authorize]
        [HttpPost("expo-token")]
        public ActionResult SaveExpoPushToken([FromQuery] string expoToken)
        {
            if (string.IsNullOrWhiteSpace(expoToken))
            {
                return BadRequest("Expo token is required.");
            }

            accountService.SaveExpoPushToken(expoToken);
            return Ok(new { Message = "Expo token saved successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var login = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                UserId = userId,
                Login = login
            });
        }
    }
}
