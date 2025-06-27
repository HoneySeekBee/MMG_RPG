using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Repositories.Interfaces;
using MMG_API.Services;

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
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("이메일 또는 비밀번호가 잘못되었습니다.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}
