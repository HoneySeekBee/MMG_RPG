using ChatPacket;
using ServerCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace MMG_Chat_Server
{
    public class Redis
    {
        public IDatabase db { get; set; }
        public Redis(IDatabase _db)
        {
            db = _db;
        }

        public void Save_RoomChat(int _charId, string _message, string _nickName, DateTime dateTime, int roomId)
        {
            long unixTimeSeconds = new DateTimeOffset(dateTime).ToUnixTimeSeconds();

            // 1. Redis에 채팅 기록 저장
            string dateKey = dateTime.ToString("yyyy-MM-dd");
            string key = $"chat:{dateKey}:room:{roomId}";
            string data = $"{dateTime:O}|{_charId}|{_nickName}|{_message}";

            // Redis List에 푸시
            db.ListRightPush(key, data);

            // 7일 후 자동 만료
            db.KeyExpire(key, TimeSpan.FromDays(7));
        }
        public void BackupCheck()
        {
            Task.Run(async () =>
            {
                DateTime lastRunDate = DateTime.MinValue; // 마지막으로 백업한 날짜

                while (true)
                {
                    try
                    {
                        var now = DateTime.UtcNow;
                        bool alreadyBackedUp = await db.KeyExistsAsync($"ChatBackup:{DateTime.Today:yyyy-MM-dd}");
                        if (!alreadyBackedUp && now.Hour >= 2)
                        {
                            await BackupChatLogsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ChatBackup] Error: {ex}");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(30)); // 30분마다 체크
                }
            });
        }
        public async Task BackupChatLogsAsync()
        {
            var endpoints = db.Multiplexer.GetEndPoints();
            var server = db.Multiplexer.GetServer(endpoints.First());

            DateTime today = DateTime.UtcNow.Date;

            // 모든 chat 키 가져오기
            foreach (var key in server.Keys(pattern: "chat:*:room:*"))
            {
                string keyStr = key.ToString();

                // chat:2025-07-27:room:1 → 2025-07-27 부분만 추출
                string[] parts = keyStr.Split(':');
                if (parts.Length < 3)
                    continue;

                if (!DateTime.TryParse(parts[1], out DateTime date))
                    continue;

                // 오늘 날짜는 건너뛰기
                if (date.Date == today)
                    continue;

                // 이미 백업한 날짜면 건너뛰기
                bool alreadyBackedUp = await db.KeyExistsAsync($"ChatBackup:{date:yyyy-MM-dd}");
                if (alreadyBackedUp)
                    continue;

                // 날짜별 폴더
                string backupFolder = Path.Combine("Backup", date.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(backupFolder);

                Console.WriteLine($"[ChatBackup] {date:yyyy-MM-dd} 백업 시작");

                // 해당 날짜+room 키 전체
                string pattern = $"chat:{date:yyyy-MM-dd}:room:*";
                var keys = server.Keys(pattern: pattern).ToArray();

                foreach (var dateKey in keys)
                {
                    var messages = await db.ListRangeAsync(dateKey);
                    string fileName = Path.Combine(
                        backupFolder,
                        dateKey.ToString().Replace(':', '_') + ".txt"
                    );
                    await File.WriteAllLinesAsync(fileName, messages.Select(v => v.ToString()));

                    // 백업 후 Redis 키 삭제
                    await db.KeyDeleteAsync(dateKey);
                }

                // ZIP 압축
                string zipPath = Path.Combine("Backup", $"ChatBackup_{date:yyyy_MM_dd}.zip");
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(backupFolder, zipPath);
                Directory.Delete(backupFolder, true);

                Console.WriteLine($"[ChatBackup] {date:yyyy-MM-dd} 백업 완료 -> {zipPath}");

                // 백업 완료 표시
                await db.StringSetAsync($"ChatBackup:{date:yyyy-MM-dd}", "done", TimeSpan.FromDays(30));
            }
        }
    }
}
