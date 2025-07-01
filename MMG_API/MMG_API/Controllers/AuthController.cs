using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Repositories.Interfaces;
using MMG_API.Services;
using System.Security.Claims;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly MMGDbContext _db; 
        private readonly IJwtTokenService _jwtService;


        public AuthController(MMGDbContext db, IJwtTokenService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("이메일 또는 비밀번호가 잘못되었습니다.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            var principal = _jwtService.ValidateToken(token, out string reason);

            if (principal == null)
                return Ok(new { IsValid = false, Reason = reason });

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var email = principal.FindFirst(ClaimTypes.Name)?.Value;

            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Ok(new { IsValid = false, Reason = "not_found" });

            return Ok(new
            {
                IsValid = true,
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
            });
        }

        
    }
}
