using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMG_API.Data;
using MMG_API.DTOs;
using MMG_API.Models;

namespace MMG_API.Controllers.Quest
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public QuestController(MMGDbContext db)
        {
            _db = db;
        }

        // [1] 전체 목록 조회
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quests = await _db.Quests.ToListAsync();

            var result = quests.Select(q => new QuestDto
            {
                QuestId = q.QuestId,
                Title = q.Title,
                Description = q.Description,
                IconCode = q.IconCode,
                Type = q.Type,
                SortOrder = q.SortOrder,
                MinLevel = q.MinLevel,
                PrevQuestIds = q.PrevQuestIds,
                StartTriggerType = q.StartTriggerType,
                StartNpcId = q.StartNpcId,
                EndTriggerType = q.EndTriggerType,
                EndNpcId = q.EndNpcId
            }).ToList();

            return Ok(result);
        }
        // [2] 단일 조회
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var quest = await _db.Quests.FindAsync(id);
            if (quest == null)
                return NotFound();

            return Ok(quest);
        }
        // [3] 생성
        [HttpPost]
        public async Task<IActionResult> Create(QuestDto dto)
        {
            var entity = new QuestEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                IconCode = dto.IconCode,
                Type = dto.Type,
                SortOrder = dto.SortOrder,
                MinLevel = dto.MinLevel,
                PrevQuestIds = dto.PrevQuestIds,
                StartTriggerType = dto.StartTriggerType,
                StartNpcId = dto.StartNpcId,
                EndTriggerType = dto.EndTriggerType,
                EndNpcId = dto.EndNpcId
            };

            _db.Quests.Add(entity);
            await _db.SaveChangesAsync();

            // 여기에 NpcQuestLinkTable 처리 추가 예정
            return Ok(entity);
        }
        // [4] 삭제
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var quest = await _db.Quests.FindAsync(id);
            if (quest == null)
                return NotFound();

            _db.Quests.Remove(quest);
            await _db.SaveChangesAsync();
            return Ok();
        }
        
        // [5] 퀘스트 수정
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, QuestDto dto)
        {
            var quest = await _db.Quests.FindAsync(id);
            if (quest == null)
                return NotFound();

            quest.Title = dto.Title;
            quest.Description = dto.Description;
            quest.IconCode = dto.IconCode;
            quest.Type = dto.Type;
            quest.SortOrder = dto.SortOrder;
            quest.MinLevel = dto.MinLevel;
            quest.PrevQuestIds = dto.PrevQuestIds;
            quest.StartTriggerType = dto.StartTriggerType;
            quest.StartNpcId = dto.StartNpcId;
            quest.EndTriggerType = dto.EndTriggerType;
            quest.EndNpcId = dto.EndNpcId;

            await _db.SaveChangesAsync();

            // 추후 NpcQuestLinkTable도 수정할 예정이면 여기에 추가
            return Ok();
        }
    }
}
