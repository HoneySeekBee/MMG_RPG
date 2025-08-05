using MMG_AdminTool.Models.Enums;
using System.ComponentModel.DataAnnotations;
using static MMG_AdminTool.Controllers.QuestCreatorController;

namespace MMG_AdminTool.Models
{
    public class QuestViewModel
    {
        public int QuestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int? IconCode { get; set; }
        [Display(Name = "퀘스트 타입")]
        [Required]
        public QuestType Type { get; set; }
        public int SortOrder { get; set; }
        [Display(Name = "최소 레벨")]
        [Range(1, 200, ErrorMessage = "최소 레벨은 1부터 200 사이여야 합니다.")]
        public int MinLevel { get; set; } = 1;

        public List<QuestSummaryViewModel> SelectedPrevQuests { get; set; } = new(); 
        public List<QuestSummaryViewModel> AllQuests { get; set; } = new();
        public List<NpcSummaryViewModel> AllNpcs { get; set; } = new();
        public string? PrevQuestIds { get; set; }

        public int StartTriggerType { get; set; }
        public int? StartNpcId { get; set; }
        public int EndTriggerType { get; set; }
        public int? EndNpcId { get; set; }
    }
    public class QuestPreviewDto
    {
        public int QuestId { get; set; }
        public string Title { get; set; }
        public int MinLevel { get; set; }
    }

}
