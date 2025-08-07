namespace MMG_API.Models
{
    public class NpcSpawnEntity
    {
        public int SpawnId { get; set; }     // PK
        public int TemplateId { get; set; }  // 참조 (NpcTemplate)
        public int MapId { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public int DirY { get; set; }
    }
}
