namespace MMG_AdminTool.Models
{
    public class NpcSpawnViewModel
    {
        public int SpawnId { get; set; }
        public int TemplateId { get; set; }
        public int MapId { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public int DirY { get; set; }
    }
}
