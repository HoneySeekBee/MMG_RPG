using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Models;

namespace MMG_API.Controllers.Quest
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestGoalController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public QuestGoalController(MMGDbContext db)
        {
            _db = db;
        }

        // [1] 특정 QuestId에 해당하는 모든 Goal 조회
        [HttpGet("byQuest/{questId}")]
        public async Task<IActionResult> GetGoalsByQuest(int questId)
        {
            var goals = await _db.QuestGoals
                .Where(g => g.QuestId == questId)
                .ToListAsync();

            return Ok(goals);
        }

        // [2] 단일 Goal 조회
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var goal = await _db.QuestGoals.FindAsync(id);
            if (goal == null)
                return NotFound();

            return Ok(goal);
        }

        // [3] 생성
        [HttpPost]
        public async Task<IActionResult> Create(QuestGoalDto dto)
        {
            var goal = new QuestGoalEntity
            {
                QuestId = dto.QuestId,
                GoalType = dto.GoalType,
                TargetId = dto.TargetId,
                Count = dto.Count
            };

            _db.QuestGoals.Add(goal);
            await _db.SaveChangesAsync();
            return Ok(goal);
        }

        // [4] 수정
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, QuestGoalDto dto)
        {
            var goal = await _db.QuestGoals.FindAsync(id);
            if (goal == null)
                return NotFound();

            goal.QuestId = dto.QuestId;
            goal.GoalType = dto.GoalType;
            goal.TargetId = dto.TargetId;
            goal.Count = dto.Count;

            await _db.SaveChangesAsync();
            return Ok(goal);
        }

        // [5] 삭제
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _db.QuestGoals.FindAsync(id);
            if (goal == null)
                return NotFound();

            _db.QuestGoals.Remove(goal);
            await _db.SaveChangesAsync();
            return Ok();
        }

    }
}
