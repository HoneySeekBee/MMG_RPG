using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Models;

namespace MMG_API.Controllers.Quest
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestRewardController : ControllerBase
    {
        private readonly MMGDbContext _db;

        public QuestRewardController(MMGDbContext db)
        {
            _db = db;
        }

        // [1] Read :
        // (1) 전체 퀘스트 보상 조회
        [HttpGet]
        public IActionResult GetAllQuestRewards()
        {
            var rewards = _db.QuestRewards.AsNoTracking().ToList();
            return Ok(rewards);
        }
        // (2) 특정 퀘스트 보상 조회  
        [HttpGet("{questId}")]
        public async Task<IActionResult> GetRewardsByQuest(int questId)
        {
            var reward = await _db.QuestRewards.AsNoTracking().SingleOrDefaultAsync(r => r.QuestId == questId);

            if (reward == null)
                return NotFound();

            return Ok(reward);
        }

        // [2] Create 
        [HttpPost]
        public async Task<IActionResult> Create(QuestRewardDto dto)
        {
            var reward = new QuestRewardEntity
            {
                QuestId = dto.QuestId,
                Exp = dto.Exp,
                JsonReward = dto.JsonReward,
            };

            _db.QuestRewards.Add(reward);
            await _db.SaveChangesAsync();
            return Ok(reward);
        }
        // [3] Update
        [HttpPut("{questId}")]
        public async Task<IActionResult> Update(int questId, QuestRewardDto dto)
        {
            var reward = await _db.QuestRewards
                          .SingleOrDefaultAsync(r => r.QuestId == questId);
            if (reward == null)
                return NotFound();

            reward.Exp = dto.Exp;
            reward.JsonReward = dto.JsonReward;

            await _db.SaveChangesAsync();

            return NoContent();
        }
        // [5] 삭제
        [HttpDelete("{questId}")]
        public async Task<IActionResult> Delete(int questId)
        {
            var reward = await _db.QuestRewards
                          .SingleOrDefaultAsync(r => r.QuestId == questId);
            if (reward == null)
                return NotFound();

            _db.QuestRewards.Remove(reward);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
