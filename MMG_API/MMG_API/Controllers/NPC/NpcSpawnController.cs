using Microsoft.AspNetCore.Mvc;
using MMG_API.Data;
using MMG_API.Models;

namespace MMG_API.Controllers.NPC
{
    [ApiController]
    [Route("api/[controller]")]
    public class NpcSpawnController : ControllerBase
    {

        private readonly MMGDbContext _db;
        public NpcSpawnController(MMGDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var spawns = _db.NpcSpawns.ToList();
            return Ok(spawns);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var spawn = _db.NpcSpawns.FirstOrDefault(s => s.SpawnId == id);
            if (spawn == null)
                return NotFound();
            return Ok(spawn);
        }

        [HttpPost]
        public IActionResult Create([FromBody] NpcSpawnEntity newSpawn)
        {
            _db.NpcSpawns.Add(newSpawn);
            _db.SaveChanges();
            return Ok(newSpawn);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] NpcSpawnEntity updated)
        {
            var spawn = _db.NpcSpawns.FirstOrDefault(s => s.SpawnId == id);
            if (spawn == null)
                return NotFound();

            spawn.TemplateId = updated.TemplateId;
            spawn.MapId = updated.MapId;
            spawn.PosX = updated.PosX;
            spawn.PosY = updated.PosY;
            spawn.PosZ = updated.PosZ;
            spawn.DirY = updated.DirY;

            _db.SaveChanges();
            return Ok(spawn);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var spawn = _db.NpcSpawns.FirstOrDefault(s => s.SpawnId == id);
            if (spawn == null)
                return NotFound();

            _db.NpcSpawns.Remove(spawn);
            _db.SaveChanges();
            return Ok();
        }
    }
}
