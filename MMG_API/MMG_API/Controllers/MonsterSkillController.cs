using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonsterSkillController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public MonsterSkillController(MMGDbContext db)
        {
            _db = db;
        }

        [HttpPost("upload-all")]
        public async Task<IActionResult> UploadMonsterSkills([FromBody] List<MonsterSkillDto> skillDtos)
        {
            {
                if (skillDtos == null || skillDtos.Count == 0)
                    return BadRequest("데이터가 없습니다");

                int monsterId = skillDtos[0].MonsterId;

                try
                {
                    // [1] 기존 데이터 삭제 후 저장
                    var existing = await _db.MonsterSkills
                                            .Where(ms => ms.MonsterId == monsterId)
                                            .ToListAsync();


                    if (existing.Count > 0)
                    {
                        _db.MonsterSkills.RemoveRange(existing);
                        await _db.SaveChangesAsync(); // 삭제 먼저 커밋
                    }
                    // [2] 새 스킬 추가
                    var newSkills = skillDtos.Select(dto => dto.ToEntity()).ToList();
                    await _db.MonsterSkills.AddRangeAsync(newSkills);
                    await _db.SaveChangesAsync();

                    return Ok("몬스터 스킬 저장 완료");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[몬스터 스킬 저장 오류] {ex.Message}");
                    return StatusCode(500, $"서버 오류: {ex.Message}");
                }
            }
        }
        [HttpGet("{monsterId}")]
        public async Task<ActionResult<List<MonsterSkillDto>>> GetMonsterSkills(int monsterId)
        {
            var skills = await _db.MonsterSkills
                .Where(cs => cs.MonsterId == monsterId)
                .Select(cs => new MonsterSkillDto
                {
                    MonsterId = cs.MonsterId,
                    SkillId = cs.SkillId,
                    Frequency = cs.Frequency,
                    InputType = cs.InputType,
                })
                .ToListAsync();

            return Ok(skills);
        }
    }
}
