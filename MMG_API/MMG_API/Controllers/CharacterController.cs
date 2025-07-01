using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Models;
using MMG_API.Repositories.Interfaces;
using System.Reflection;
using System.Security.Claims;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController: ControllerBase
    {
        private readonly MMGDbContext _db;
        private readonly IJwtTokenService _jwtService;
        public CharacterController(MMGDbContext db, IJwtTokenService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetCharacterList()
        {
            Console.WriteLine("캐릭터 리스트 불러오기");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var characters = await _db.Characters
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(characters);
        }
        [HttpGet("check-name")]
        public async Task<IActionResult> CheckDuplicateNickname([FromQuery] string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                return BadRequest("닉네임이 비어있습니다.");

            bool isDuplicate = await _db.Characters.AnyAsync(c => c.CharacterName == nickname);

            return Ok(new { isDuplicate });
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. 동일 슬롯에 이미 캐릭터가 있는지 확인
            bool exists = await _db.Characters.AnyAsync(c =>
                c.UserId == userId &&
                c.SlotNumber == dto.SlotNumber &&
                !c.IsDeleted); // 삭제된 건 허용

            if (exists)
                return Conflict($"이미 해당 슬롯 {dto.SlotNumber}에 캐릭터가 존재합니다.");

            // 2. 새 캐릭터 생성
            Character character = new Character
            {
                UserId = userId,
                SlotNumber = dto.SlotNumber,
                CharacterName = dto.CharacterName,
                Gender = dto.Gender,
                Class = dto.Class,
                AppearanceCode = dto.AppearanceCode,
                CreatedAt = DateTime.UtcNow,
                LastPlayedAt = null,
                IsDeleted = false
            };

            _db.Characters.Add(character);
            await _db.SaveChangesAsync();

            return Ok(new { message = "캐릭터 생성 완료", characterId = character.Id });
        }
    }
}
