using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.Models;
using MMG_API.Repositories.Interfaces;

namespace MMG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly MMGDbContext _db;
        public ItemsController(MMGDbContext db)
        {
            _db = db;
        }  
        
        // [1] 모든 아이템 조회
        [HttpGet]
        public IActionResult GetAll()
        {
            var items = _db.Items.ToList();
            return Ok(items);
        }

        // [2] 특정 아이템 조회
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var item = _db.Items.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // [3] 아이템 생성
        [HttpPost]
        public IActionResult Create([FromBody] ItemEntity newItem)
        {
            _db.Items.Add(newItem);
            _db.SaveChanges();
            return Ok(newItem);
        }

        // [4] 아이템 수정
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ItemEntity updatedItem)
        {
            var item = _db.Items.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
                return NotFound();

            // 필요한 필드 업데이트
            item.Name = updatedItem.Name;
            item.Description = updatedItem.Description;
            item.Type = updatedItem.Type;
            item.IconId = updatedItem.IconId;
            item.ModelId = updatedItem.ModelId;
            item.RequiredLevel = updatedItem.RequiredLevel;
            item.JsonStatModifiers = updatedItem.JsonStatModifiers;
            item.JsonRequiredStats = updatedItem.JsonRequiredStats;
            item.JsonUseableEffect = updatedItem.JsonUseableEffect;

            _db.SaveChanges();
            return Ok(item);
        }

        // [5] 아이템 삭제
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _db.Items.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
                return NotFound();

            _db.Items.Remove(item);
            _db.SaveChanges();
            return Ok();
        }
    }
}
