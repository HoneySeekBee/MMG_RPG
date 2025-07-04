using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Data
{
    internal class EFCharacterDB
    {
        // 캐릭터 정보 추가
        public static void SaveCharacterData(int userId, CharacterDto data)
        {
            using var db = new AppDbContext();
            try
            {
                db.Characters.Add(data);
                db.SaveChanges();

                Console.WriteLine($"[DB] 유저 {userId}의 신규 캐릭터 정보 저장");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 오류] 유저 {userId} 캐릭터 생성 저장 중 예외 발생: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");

            }

        }

        // 캐릭터 정보 업데이트
        public static void UpdateCharacterData(int userId, CharacterDto data)
        {
            using var db = new AppDbContext();
            try
            {
                // 일단 Id를 사용해서 찾아보자. 
                var existCharacter = db.Characters
     .FirstOrDefault(x => x.Id == data.Id && x.UserId == userId);

                if (existCharacter == null)
                {
                    // 없으니 새로 저장 
                    Console.WriteLine($"[DB 경고] UserId : {userId} 캐릭터 {data.Id}이 아이디가 존재하지 않음");
                    return;
                }

                existCharacter.SlotNumber = data.SlotNumber;
                existCharacter.CharacterName = data.CharacterName;
                existCharacter.Class = data.Class;
                existCharacter.AppearanceCode = data.AppearanceCode;
                existCharacter.LastUpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 오류] 유저 {userId} 캐릭터 데이터 저장 중 예외 발생: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");
            }
        }

        public static List<CharacterDto> LoadCharacterListData(int userId)
        {
            using (var db = new AppDbContext())
            {
                return db.Characters
                    .Where(obj => obj.UserId == userId && obj.isDeleted == false)
                    .AsNoTracking()
                    .ToList();
            }
        }
    }
}
