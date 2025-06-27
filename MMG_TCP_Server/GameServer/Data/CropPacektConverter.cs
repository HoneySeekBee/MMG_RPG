using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public  static class CropPacektConverter
    {
        public static PlantedCrop ToProto(PlantedCropDto item)
        {
            float plantedAt = (float)(item.PlantedAt.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
            float lastUpdateAt = (float)(item.LastUpdateAt.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;

            Console.WriteLine($"[ToProto] PlantedAt double = {(item.PlantedAt.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds}, float = {plantedAt}");

            return new PlantedCrop
            {
                Id = item.Id,
                OwnerUserId = item.OwnerUserId,
                CropId = item.CropId,
                PosX = item.PosX,
                PosY = item.PosY,
                GrowthStage = item.GrowthStage,
                GrowthTimer = item.GrowthTimer,
                PlantedAt = plantedAt,
                LastUpdateAt = lastUpdateAt,
            };
        }
    }
}
