namespace MMG_API.Models
{
    public class NpcQuestLinkEntity
    {
        public int NpcTemplateId { get; set; }
        public int QuestId { get; set; }
        public int LinkType { get; set; } // 0: 시작, 1: 완료
    }
}
