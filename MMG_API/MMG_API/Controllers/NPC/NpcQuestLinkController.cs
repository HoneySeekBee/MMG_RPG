using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.Models;

namespace MMG_API.Controllers.NPC
{
    [ApiController]
    [Route("api/[controller]")]
    public class NpcQuestLinkController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public NpcQuestLinkController(MMGDbContext db)
        {
            _db = db;
        }     
        // [1] 전체 조회
        [HttpGet]
        public IActionResult GetAll()
        {
            var links = _db.NpcQuestLinks.ToList();
            return Ok(links);
        }
        [HttpGet("quest/{questId}")]
        public IActionResult GetByQuestId(int questId)
        {
            var links = _db.NpcQuestLinks
                           .Where(l => l.QuestId == questId)
                           .ToList();
            return Ok(links);
        }
        // [2] 특정 NPC 템플릿에 해당하는 링크 조회
        [HttpGet("npc/{npcTemplateId}")]
        public IActionResult GetByNpcTemplate(int npcTemplateId)
        {
            var links = _db.NpcQuestLinks
                           .Where(l => l.NpcTemplateId == npcTemplateId)
                           .ToList();
            return Ok(links);
        }

        // [3] 생성 (Quest 연결 추가)
        [HttpPost]
        public IActionResult Create([FromBody] NpcQuestLinkEntity link)
        {
            // 이미 존재하는지 확인
            var exists = _db.NpcQuestLinks
                .Any(l => l.NpcTemplateId == link.NpcTemplateId && l.QuestId == link.QuestId);

            if (exists)
                return Conflict("이미 존재하는 링크입니다.");

            _db.NpcQuestLinks.Add(link);
            _db.SaveChanges();
            return Ok(link);
        }

        // [4] 수정 (LinkType만 수정 가능)
        [HttpPut("{npcTemplateId}/{questId}")]
        public IActionResult Update(int npcTemplateId, int questId, [FromBody] NpcQuestLinkEntity updated)
        {
            var link = _db.NpcQuestLinks
                .FirstOrDefault(l => l.NpcTemplateId == npcTemplateId && l.QuestId == questId);

            if (link == null)
                return NotFound();

            link.LinkType = updated.LinkType;
            _db.SaveChanges();
            return Ok(link);
        }

        // [5] 삭제
        [HttpDelete("{npcTemplateId}/{questId}")]
        public IActionResult Delete(int npcTemplateId, int questId)
        {
            var link = _db.NpcQuestLinks
                .FirstOrDefault(l => l.NpcTemplateId == npcTemplateId && l.QuestId == questId);

            if (link == null)
                return NotFound();

            _db.NpcQuestLinks.Remove(link);
            _db.SaveChanges();
            return Ok();
        }
    }
}
