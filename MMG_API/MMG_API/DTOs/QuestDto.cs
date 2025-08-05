namespace MMG_API.DTOs
{
    public class QuestDto
    {
        public int QuestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? IconCode { get; set; }
        public int Type { get; set; }
        public int SortOrder { get; set; }
        public int MinLevel { get; set; }
        public string? PrevQuestIds { get; set; }
        public int StartTriggerType { get; set; }
        public int? StartNpcId { get; set; }
        public int EndTriggerType { get; set; }
        public int? EndNpcId { get; set; }
    }
}
