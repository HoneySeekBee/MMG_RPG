using GameServer.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    internal class EFVillageDB
    {
        // 최신 데이터를 백업으로 이동
        public static void BackupCurrent(int userId)
        {
            using (var db = new AppDbContext())
            {
                var currentData = db.PlacedObjects
                                    .Where(obj => obj.UserId == userId && obj.Version == 0)
                                    .ToList();

                foreach (var obj in currentData)
                {
                    obj.Version = 1; // 백업 버전으로 이동
                }

                db.SaveChanges();
            }
        }

        // 기존 백업 데이터 삭제
        public static void DeleteBackup(int userId)
        {
            using (var db = new AppDbContext())
            {
                var backupData = db.PlacedObjects
                                   .Where(obj => obj.UserId == userId && obj.Version == 1)
                                   .ToList();

                db.PlacedObjects.RemoveRange(backupData);
                db.SaveChanges();
            }
        }

        // 새로운 데이터 저장
        public static void SaveVillageData(int userId, List<PlacedObjectDTO> newObjects)
        {
            using var db = new AppDbContext();

            try
            {
                // 기존 최신 데이터 삭제 (Version == 1)
                var oldObjects = db.PlacedObjects
                    .Where(o => o.UserId == userId && o.Version == 1)
                    .ToList();

                if (oldObjects.Any())
                {
                    db.PlacedObjects.RemoveRange(oldObjects);
                }

                // 새로운 데이터 삽입
                db.PlacedObjects.AddRange(newObjects);
                db.SaveChanges();

                Console.WriteLine($"[DB] 유저 {userId}의 마을 데이터를 {newObjects.Count}개 저장 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 오류] 유저 {userId} 데이터 저장 중 예외 발생: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");
            }
        }
        public static void SavePlantedCrop(int userId, PlantedCropDto plantedCrop)
        {
            using var db = new AppDbContext();
            try
            {
                db.PlantedCrop.Add(plantedCrop);
                db.SaveChanges();

                Console.WriteLine($"[DB] 유저 {userId}의 작물 정보 {plantedCrop.CropId} 저장");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 오류] 유저 {userId} 데이터 저장 중 예외 발생: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");
            }
        }
        public static void UpdatePlantedCrop(int cropId, PlantedCropDto plantedCrop)
        {
            using var db = new AppDbContext();
            try
            {
                var existingCrop = db.PlantedCrop.FirstOrDefault(c => c.Id == cropId);

                if (existingCrop == null)
                {
                    Console.WriteLine($"[DB 경고] 작물 {cropId} 존재하지 않음");
                    return;
                }

                existingCrop.IsHarvest = plantedCrop.IsHarvest;
                existingCrop.HarvestTime = plantedCrop.HarvestTime;

                db.SaveChanges();

                Console.WriteLine($"[DB] 작물 {cropId} 정보 업데이트 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 오류] 작물 번호 {cropId} 데이터 저장 중 예외 발생: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");
            }
        }
        public static bool CanUpdatePlantedCrop(int userId, int cropId)
        {
            using var db = new AppDbContext();

            return db.PlantedCrop.Any(crop =>
                crop.Id == cropId &&
                crop.OwnerUserId == userId &&
                crop.IsHarvest == null);
        }
        // 마을 불러오기
        public static List<PlacedObjectDTO> LoadVillageData(int userId)
        {
            using (var db = new AppDbContext())
            {
                return db.PlacedObjects
                         .Where(obj => obj.UserId == userId && obj.Version == 0)
                         .AsNoTracking()
                         .ToList();
            }
        }
        public static List<PlantedCropDto> LoadPlantedCrops(int userId)
        {
            using var db = new AppDbContext();
            return db.PlantedCrop
                .Where(c => c.OwnerUserId == userId && c.IsHarvest == null)
                .AsNoTracking()
                .ToList();
        }
    }
}
