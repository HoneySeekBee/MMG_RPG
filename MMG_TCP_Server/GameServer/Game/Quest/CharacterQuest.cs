using GamePacket;
using GameServer.Data;
using GameServer.Data.DataManager;
using GameServer.Game.Object;
using QuestPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Data.API;

namespace GameServer.Game.Quest
{
    public class CharacterQuest
    {
        private CharacterObject MyCharacter;
        public List<Quest> AvailableQuest = new List<Quest>();
        public List<QuestCounter> ProgressQuest = new List<QuestCounter>();
        public List<Quest> CompletedQuest = new List<Quest>();
        private HashSet<int> _completedSet = new();
        private HashSet<int> _progressSet = new();
        private HashSet<int> _availableSet = new();

        #region Ongoing Quests
        private Dictionary<(QuestGoalType, int), List<QuestCountChecker>> OnGoingDictionary
            = new Dictionary<(QuestGoalType, int), List<QuestCountChecker>>();

        private readonly Dictionary<int, Dictionary<string, int>> _pendingDelta = new();
        private readonly HashSet<int> _dirtyQuests = new();
        private readonly object _sync = new();

        #endregion
        public CharacterQuest(CharacterObject myCharacter)
        {
            MyCharacter = myCharacter;
            AvailableQuest = new List<Quest>();
            ProgressQuest = new List<QuestCounter>();
            CompletedQuest = new List<Quest>();
            OnGoingDictionary = new Dictionary<(QuestGoalType, int), List<QuestCountChecker>>();
        }
        public async Task InitAsync()
        {
            await GetQuest();
        }
        // [1] 퀘스트 전부 받기 
        public async Task GetQuest()
        {
            // 전체 퀘스트 중 진행 가능한 퀘스트 가지고 오기 ( 레벨 기준 만 보고 가지고 오기 )
            // 조건 : ( 레벨, 선행 퀘스트 ) 
            var api = new API();
            List<UserQuestDto> dtos = await api.GetUserQuest(MyCharacter.objectInfo.Id);
            if (dtos == null)
            {
                Console.WriteLine("[Error] GetQuest");
                return;
            }

            // [1] dtos들을 Status별로 나눠보자. 
            _completedSet = dtos
                .Where(q => q.Status == QuestProgressStatus.Completed)
                .Select(q => q.QuestId)
                .ToHashSet();

            _progressSet = dtos
                .Where(q => q.Status == QuestProgressStatus.Active)
                .Select(q => q.QuestId)
                .ToHashSet();

            Dictionary<int, string> ProgressState = dtos
                .Where(q => q.Status == QuestProgressStatus.Active)
                .ToDictionary(t => t.QuestId, t => t.Progress ?? string.Empty);
            // 캐릭터 아이디를 기반으로 UserQuests 테이블 값 불러오기
            List<Quest> MyQuest = QuestDataManager.GetQuest_byCondition(MyCharacter.objectStatus.Level, _completedSet);
            foreach (var q in MyQuest)
            {
                int id = q.QuestId;

                if (_completedSet.Contains(id))
                    CompletedQuest.Add(q);
                else if (_progressSet.Contains(id))
                {
                    string json = ProgressState.ContainsKey(q.QuestId) ? ProgressState[q.QuestId] : string.Empty;
                    QuestCounter Q = new QuestCounter(q, json);
                    ProgressQuest.Add(Q);
                    Q.AddQuest(ref OnGoingDictionary);
                }
                else
                {
                    _availableSet.Add(id);
                    AvailableQuest.Add(q);
                }
            }
        }

        // [2] 레벨업 
        public void UpdateQuest_LevelUp(int oldLevel, int newlevel)
        {
            if (newlevel <= oldLevel)
                return;

            var candidates = QuestDataManager.GetQuest_byCondition(newlevel, _completedSet);
            var newAvaliable = candidates
                .Where(q =>
                q.QuestCondition.MinLevel > oldLevel && q.QuestCondition.MinLevel <= newlevel &&
                _completedSet.Contains(q.QuestId) == false &&
                _progressSet.Contains(q.QuestId) == false &&
                _availableSet.Contains(q.QuestId) == false)
                .ToList();

            if (newAvaliable.Count == 0)
                return;

            AvailableQuest.AddRange(newAvaliable);
            foreach (var q in newAvaliable)
                _availableSet.Add(q.QuestId);

            // 여기서 새로운 가능한 퀘스트가 있으면 클라로 보내준다.
            Console.WriteLine($"[CharacterQuest] - New Available Quest {newAvaliable.Count}");
        }

        // [3] 퀘스트 업데이트
        // (1) 주기적(60초 마다) 업데이트 하기
        public async Task UpdateQuest_Renew()
        {
            // 스냅샷
            Dictionary<int, Dictionary<string, int>> snapshot;
            lock (_sync)
            {
                if (_dirtyQuests.Count == 0) return;
                snapshot = _pendingDelta.ToDictionary(
                    kv => kv.Key,
                    kv => new Dictionary<string, int>(kv.Value, StringComparer.Ordinal));
                _pendingDelta.Clear();
                _dirtyQuests.Clear();
            }

            var api = new API();
            var req = new ReportQuestProgressBatchRequest
            {
                BatchId = Guid.NewGuid().ToString(),
                Items = snapshot.Select(kv => new ReportQuestProgressBatchRequest.Item
                {
                    QuestId = kv.Key,
                    ProgressDelta = kv.Value
                }).ToList()
            };

            bool ok = false;
            try { ok = await api.ReportProgressBatchAsync(MyCharacter.objectInfo.Id, req); }
            catch (Exception ex)
            {
                Console.WriteLine($"[Renew] Batch failed: {ex}");
            }

            if (!ok)
            {
                // 실패 → 스냅샷 복원
                lock (_sync)
                {
                    foreach (var (qid, map) in snapshot)
                    {
                        if (!_pendingDelta.TryGetValue(qid, out var cur))
                            _pendingDelta[qid] = cur = new Dictionary<string, int>(StringComparer.Ordinal);
                        foreach (var (k, v) in map)
                            if (!cur.TryAdd(k, v)) cur[k] += v;
                        _dirtyQuests.Add(qid);
                    }
                }
            }
        }

        // [4] 퀘스트 완료  
        public void CompleteQuest(int questId)
        {
            // 진행 목록에서 찾기

            int idx = ProgressQuest.FindIndex(x => x.ThisQuest.QuestId == questId);
            if (idx < 0) return;

            var qc = ProgressQuest[idx];

            if (qc.IsCompleted == false)
                return;
            // (1) 역인덱스에서 이 퀘스트 목표들 제거
            qc.RemoveQuest(ref OnGoingDictionary);

            // (2) 리스트 이동
            ProgressQuest.RemoveAt(idx);
            CompletedQuest.Add(qc.ThisQuest);

            // (3) 보조 집합 갱신
            _progressSet.Remove(questId);
            _completedSet.Add(questId);
            _availableSet.Remove(questId); // 혹시 남아있을 수 있으니 방어적으로 제거

            // (4) 테이블 저장 및 클라에 전달 

        }

        // 갱신하기 
        public void ReportQuest(QuestGoalType goalType, int goalId, int add = 1)
        {
            var key = (goalType, goalId);
            if (!OnGoingDictionary.TryGetValue(key, out var bucket) || bucket.Count == 0)
                return;

            var justCompleted = new List<int>();
            foreach (var chk in bucket)
            {
                if (chk.NowCount >= chk.GoalCount) continue;

                int before = chk.NowCount;
                int after = Math.Min(chk.NowCount + add, chk.GoalCount);
                int applied = after - before;
                if (applied == 0) continue;

                chk.NowCount = after;
                BufferDelta(chk.QuestId, goalType, goalId, applied);

                var qc = ProgressQuest.FirstOrDefault(x => x.ThisQuest.QuestId == chk.QuestId);
                if (qc != null && qc.IsCompleted)
                    justCompleted.Add(chk.QuestId);
            }
            foreach (var qid in justCompleted)
                CompleteLocally(qid);
        }
        private void CompleteLocally(int questId)
        {
            int idx = ProgressQuest.FindIndex(x => x.ThisQuest.QuestId == questId);
            if (idx < 0) return;

            var qc = ProgressQuest[idx];
            if (qc.ThisQuest.QuestEnd.Type == QuestTriggerType.Auto)
            {
                CompleteQuest(questId);
            }
            else
            {
                // UI로 완료 가능 할려주기 
            }
        }
        private void BufferDelta(int questId, QuestGoalType type, int targetId, int applied)
        {
            if (applied == 0) return;

            var key = QuestProgressUtil.MakeKey(type, targetId); // "kill:123" / "collect:456"

            lock (_sync)
            {
                if (!_pendingDelta.TryGetValue(questId, out var map))
                    _pendingDelta[questId] = map = new Dictionary<string, int>(StringComparer.Ordinal);

                if (!map.TryAdd(key, applied)) map[key] += applied;
                _dirtyQuests.Add(questId);
            }
        }
    }
}
