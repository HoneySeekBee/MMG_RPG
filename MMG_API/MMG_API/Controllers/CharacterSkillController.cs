using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterSkillController: ControllerBase
    {
        private readonly MMGDbContext _db;
        private readonly IJwtTokenService _jwtService;
        public CharacterSkillController(MMGDbContext db, IJwtTokenService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }
        [HttpGet("{characterId}")]
        public async Task<ActionResult<List<CharacterSkillDto>>> GetCharacterSkills(int characterId)
        {
            var skills = await _db.CharacterSkills
                .Where(cs => cs.CharacterId == characterId)
                .Select(cs => new CharacterSkillDto
                {
                    CharacterId = cs.CharacterId,
                    SkillId = cs.SkillId,
                    InputType = cs.InputType,
                    SkillLevel = cs.SkillLevel,
                    SlotIndex = cs.SlotIndex
                })
                .ToListAsync();

            return Ok(skills);
        }
    }
}
