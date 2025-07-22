using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonsterController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public MonsterController(MMGDbContext db)
        {
            _db = db;
        }

        // [1] DB에 몬스터 Data 저장
        [HttpPost("upload")]
        public async Task<IActionResult> UploadMonster([FromBody] MonsterDto dto)
        {
            var exists = await _db.Monsters.FindAsync(dto.Id);

            if (exists != null)
            {
                // 기존 몬스터 업데이트
                exists.Name = dto.Name;
                exists.Speed = dto.Speed;
                exists.ChaseRange = dto.ChaseRange;
                exists.AttackRange = dto.AttackRange;
                exists.Exp = dto.Exp;
                exists.Gold = dto.Gold;
                _db.Monsters.Update(exists);
            }
            else
            {
                // 새 몬스터 추가
                await _db.Monsters.AddAsync(dto.ToEntity());
            }

            await _db.SaveChangesAsync();
            return Ok("몬스터 저장 완료");
        }

        // [2] Monster 정보 다 불러오기 
        [HttpGet("all")]
        public async Task<ActionResult<List<MonsterWithSkillsDto>>> GetAllMonstersWithSkills()
        {
            var query = from m in _db.Monsters
                        select new MonsterWithSkillsDto
                        {
                            MonsterId = m.Id,
                            Name = m.Name,
                            HP = m.HP,
                            Speed = m.Speed,
                            ChaseRange = m.ChaseRange,
                            AttackRange = m.AttackRange,
                            Exp = m.Exp,
                            Gold = m.Gold,

                            Skills = (from ms in _db.MonsterSkills
                                      where ms.MonsterId == m.Id
                                      select new MonsterSkillDto
                                      {
                                          SkillId = ms.SkillId,
                                          Frequency = ms.Frequency,
                                          InputType = ms.InputType,
                                      }).ToList()
                        };

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("max-id")]
        public async Task<IActionResult> GetMaxId()
        {
            var maxId = await _db.Monsters.MaxAsync(s => (int?)s.Id) ?? 0;
            return Ok(maxId);
        }
    }
}
