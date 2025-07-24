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
                .Join(_db.CharacterStatuses,
                    character => character.Id,
                    status => status.CharacterId,
                    (character, status) => new CharacterSummaryDto
                    {
                        Id = character.Id,
                        UserId = character.UserId,
                        CharacterName = character.CharacterName,
                        SlotNumber = character.SlotNumber,
                        Gender = character.Gender,
                        Class = character.Class,
                        AppearanceCode = character.AppearanceCode,
                        CreatedAt = character.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        LastPlayedAt = character.LastPlayedAt.ToString(),
                        LastMapId = character.LastMapId,
                        LastSpawnPointId = character.LastSpawnPointId,
                        IsDeleted = character.IsDeleted,
                        Level = status.CharacterLevel
                    })
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
            await AddStatus(character.Id, character.CreatedAt);
            return Ok(new {
                message = "캐릭터 생성 완료", 
                characterId = character.Id 
            });
        }
        private async Task AddStatus(int characterId, DateTime createTime)
        {
            CharacterStatus characterStatus = new CharacterStatus()
            {
                CharacterId = characterId,
                CharacterLevel = 1,
                Exp = 0,
                MaxExp = 10,
                Gold = 0,
                HP = 10,
                MP = 10,
                NowHP = 10,
                NowMP = 10,
                LastUpdateAt = createTime
            };
            _db.CharacterStatuses.Add(characterStatus);
            await _db.SaveChangesAsync();
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacterById([FromRoute] int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var character = await _db.Characters.FirstOrDefaultAsync(c =>
                c.Id == id && c.UserId == userId && !c.IsDeleted);

            if (character == null)
                return NotFound("해당 캐릭터를 찾을 수 없습니다.");

            return Ok(character);
        }

        [HttpGet("status/{characterId}")]
        public IActionResult GetCharacterStatus(int characterId)
        {
            var status = _db.CharacterStatuses
                            .FirstOrDefault(s => s.CharacterId == characterId);

            if (status == null)
                return NotFound(new { message = "캐릭터 상태를 찾을 수 없습니다." });

            return Ok(new
            {
                characterId = status.CharacterId,
                level = status.CharacterLevel,
                exp = status.Exp,
                maxExp = status.MaxExp,
                gold = status.Gold,
                maxHp = status.HP,
                nowHp = status.NowHP,
                maxMp = status.MP,
                nowMp = status.NowMP,
                lastUpdated = status.LastUpdateAt
            });
        }

        [HttpPost("status/update")]
        public IActionResult UpdateCharacterStatus([FromBody] UpdateCharacterStatusRequest request)
        {
            var status = _db.CharacterStatuses
                            .FirstOrDefault(s => s.CharacterId == request.CharacterId);

            if (status == null)
                return NotFound(new { message = "해당 캐릭터의 상태 정보를 찾을 수 없습니다." });

            // 값이 들어온 항목만 업데이트
            if(request.CharacterLevel.HasValue) status.CharacterLevel = request.CharacterLevel.Value;
            if (request.NowHP.HasValue) status.HP = request.HP.Value;
            if (request.NowMP.HasValue) status.MP = request.MP.Value;
            if (request.NowHP.HasValue) status.NowHP = request.NowHP.Value;
            if (request.NowMP.HasValue) status.NowMP = request.NowMP.Value;
            if (request.Exp.HasValue) status.Exp = request.Exp.Value;
            if (request.MaxExp.HasValue) status.MaxExp = request.MaxExp.Value;
            if (request.Gold.HasValue) status.Gold = request.Gold.Value;

            status.LastUpdateAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new { message = "캐릭터 상태가 업데이트되었습니다." });
        }


    }
}
