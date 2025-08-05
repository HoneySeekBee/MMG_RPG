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

        // [1] 특정 퀘스트의 보상 목록 조회
        [HttpGet("byQuest/{questId}")]
        public async Task<IActionResult> GetRewardsByQuest(int questId)
        {
            var rewards = await _db.QuestRewards
                                   .Where(r => r.QuestId == questId)
                                   .ToListAsync();
            return Ok(rewards);
        }
        // [2] 단일 보상 조회
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var reward = await _db.QuestRewards.FindAsync(id);
            if (reward == null)
                return NotFound();

            return Ok(reward);
        }
        // [3] 생성
        [HttpPost]
        public async Task<IActionResult> Create(QuestRewardDto dto)
        {
            var reward = new QuestRewardEntity
            {
                QuestId = dto.QuestId,
                RewardType = dto.RewardType,
                ItemId = dto.ItemId,
                Count = dto.Count,
                Exp = dto.Exp
            };

            _db.QuestRewards.Add(reward);
            await _db.SaveChangesAsync();
            return Ok(reward);
        }
        // [4] 수정
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, QuestRewardDto dto)
        {
            var reward = await _db.QuestRewards.FindAsync(id);
            if (reward == null)
                return NotFound();

            reward.QuestId = dto.QuestId;
            reward.RewardType = dto.RewardType;
            reward.ItemId = dto.ItemId;
            reward.Count = dto.Count;
            reward.Exp = dto.Exp;

            await _db.SaveChangesAsync();
            return Ok(reward);
        }
        // [5] 삭제
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reward = await _db.QuestRewards.FindAsync(id);
            if (reward == null)
                return NotFound();

            _db.QuestRewards.Remove(reward);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
