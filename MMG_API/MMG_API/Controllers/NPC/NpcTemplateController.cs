using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.Models;

namespace MMG_API.Controllers.NPC
{
    [ApiController]
    [Route("api/[controller]")]
    public class NpcTemplateController : ControllerBase
    {
        private readonly MMGDbContext _db;

        public NpcTemplateController(MMGDbContext db)
        {
            _db = db;
        }
        // [1] 전체 조회
        [HttpGet]
        public IActionResult GetAll()
        {
            var templates = _db.NpcTemplates.ToList();
            return Ok(templates);
        }

        // [2] 단일 조회
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var template = _db.NpcTemplates.FirstOrDefault(t => t.TemplateId == id);
            if (template == null)
                return NotFound();
            return Ok(template);
        }

        // [3] 생성
        [HttpPost]
        public IActionResult Create([FromBody] NpcTemplateEntity newTemplate)
        {
            _db.NpcTemplates.Add(newTemplate);
            _db.SaveChanges();
            return Ok(newTemplate);
        }

        // [4] 수정
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] NpcTemplateEntity updated)
        {
            var template = _db.NpcTemplates.FirstOrDefault(t => t.TemplateId == id);
            if (template == null)
                return NotFound();

            template.Name = updated.Name;
            template.Type = updated.Type;
            template.DialogueKey = updated.DialogueKey;
            template.JsonShopItems = updated.JsonShopItems;

            _db.SaveChanges();
            return Ok(template);
        }

        // [5] 삭제
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var template = _db.NpcTemplates.FirstOrDefault(t => t.TemplateId == id);
            if (template == null)
                return NotFound();

            _db.NpcTemplates.Remove(template);
            _db.SaveChanges();
            return Ok();
        }
    }
}
