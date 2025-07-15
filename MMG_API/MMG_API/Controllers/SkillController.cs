using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using System;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public SkillController(MMGDbContext context)
        {
            _db = context;
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadSkills([FromBody] List<SkillDto> skillDtos)
        {
            foreach (var dto in skillDtos)
            {
                var exists = await _db.Skills.AnyAsync(s => s.AttackId == dto.AttackId);

                if (exists)
                    _db.Skills.Update(dto.ToEntity());
                else
                    await _db.Skills.AddAsync(dto.ToEntity());
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("upload-single")]
        public async Task<IActionResult> UploadSkill([FromBody] SkillDto dto)
        {
            var entity = dto.ToEntity();
            var exists = await _db.Skills.AnyAsync(s => s.AttackId == dto.AttackId);

            if (exists)
                _db.Skills.Update(entity);
            else
                await _db.Skills.AddAsync(entity);

            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("max-id")]
        public async Task<IActionResult> GetMaxId()
        {
            var maxId = await _db.Skills.MaxAsync(s => (int?)s.AttackId) ?? 0;
            return Ok(maxId);
        }
        [HttpGet("all")]
        public async Task<ActionResult<List<SkillDto>>> GetAllSkills()
        {
            var skills = await _db.Skills
                .Select(s => new SkillDto
                {
                    AttackId = s.AttackId,
                    AttackName = s.AttackName,
                    OwnerType = s.OwnerType,
                    WeaponType = s.WeaponType,
                    AttackType = s.AttackType,
                    Range = s.Range,
                    Angle = s.Angle,
                    Damage = s.Damage,
                    Cooldown = s.Cooldown,
                    DelayAfter = s.DelayAfter,
                    CastTime = s.CastTime
                })
                .ToListAsync();

            return Ok(skills);
        }
    }
}
