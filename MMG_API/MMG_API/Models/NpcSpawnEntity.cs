namespace MMG_API.Models
{
    public class NpcSpawnEntity
    {
        public int SpawnId { get; set; }     // PK
        public int TemplateId { get; set; }  // 참조 (NpcTemplate)
        public int MapId { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float DirY { get; set; }
    }
}
