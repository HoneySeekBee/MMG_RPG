namespace MMG_AdminTool.Models
{
    public class NpcSpawnViewModel
    {
        public int SpawnId { get; set; }
        public int TemplateId { get; set; }
        public int MapId { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float DirY { get; set; }
    }
}
