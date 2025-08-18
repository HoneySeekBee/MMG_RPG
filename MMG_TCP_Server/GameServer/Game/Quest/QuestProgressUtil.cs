using Newtonsoft.Json.Schema;
using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameServer.Game.Quest
{
    public static class QuestProgressUtil
    {
        private static readonly JsonSerializerOptions J = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static Dictionary<string, int> Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(json, J)
                    ?? new Dictionary<string, int>();
            }
            catch
            {
                try
                {
                    using var doc = JsonDocument.Parse(json!);
                    var map = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        int v = 0;
                        if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var n))
                            v = n;
                        else if (prop.Value.ValueKind == JsonValueKind.String && int.TryParse(prop.Value.GetString(), out var n2))
                            v = n2;

                        map[prop.Name] = v < 0 ? 0 : v;
                    }
                    return map;
                }
                catch
                {
                    return new();
                }
            }
        }
        public static string MakeKey(QuestGoalType type, int id) => type switch
        {
            QuestGoalType.HuntMonster => $"kill:{id}",
            QuestGoalType.GetItem => $"collect:{id}",
            _ => $"{type.ToString().ToLowerInvariant()}:{id}"
        };
        public static int GetCount(Dictionary<string, int> progress, QuestGoalType type, int id)
            => progress.TryGetValue(MakeKey(type, id), out var v) ? v : 0;
    }
}
