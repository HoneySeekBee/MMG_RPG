using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class PlantedCropDto
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; } 
        public int CropId { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }


        public int GrowthStage { get; set; }
        public float GrowthTimer { get; set; }

        public DateTime PlantedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime? HarvestTime{ get; set; }
        public bool? IsHarvest{ get; set; }
    }
}
