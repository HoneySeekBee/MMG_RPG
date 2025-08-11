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
                GoalIndex = dto.GoalIndex,
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
        [HttpGet("{questId}")]
        [ProducesResponseType(typeof(List<QuestGoalDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByQuest(int questId)
        {
            var goals = await _db.QuestGoals
                .Where(g => g.QuestId == questId)
                .OrderBy(g => g.GoalIndex)
                .Select(g => new QuestGoalDto
                {
                    QuestId = g.QuestId,
                    GoalIndex = g.GoalIndex,
                    GoalType = g.GoalType,
                    TargetId = g.TargetId,
                    Count = g.Count
                })
                .ToListAsync();

            return Ok(goals); // 없으면 [] 반환
        }
        [HttpPut("{questId}/goals")]
        public async Task<IActionResult> ReplaceAll(int questId, [FromBody] List<QuestGoalDto> goals)
        {
            Console.WriteLine($"[QuestGoalCreator] ReplaceAll - {questId}");
            // 트랜잭션으로 묶기
            using var tx = await _db.Database.BeginTransactionAsync();

            // 1) 기존 전부 삭제
            var existing = await _db.QuestGoals
                .Where(g => g.QuestId == questId)
                .ToListAsync();

            _db.QuestGoals.RemoveRange(existing);
            await _db.SaveChangesAsync();

            // 2) 새 목록(정렬 보장)으로 다시 삽입
            var entities = goals
                .OrderBy(g => g.GoalIndex)
                .Select(g => new QuestGoalEntity
                {
                    QuestId = questId,
                    GoalIndex = g.GoalIndex,
                    GoalType = g.GoalType,
                    TargetId = g.TargetId,
                    Count = g.Count
                })
                .ToList();

            await _db.QuestGoals.AddRangeAsync(entities);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return Ok(entities);
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
