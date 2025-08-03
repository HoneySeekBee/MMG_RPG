using System.ComponentModel.DataAnnotations;

namespace MMG_AdminTool.Models
{
    public class ItemViewModel
    {
        public int ItemId { get; set; }  // 수정 시 필요, 생성 시 0

        [Required(ErrorMessage = "아이템 이름은 필수입니다.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Type { get; set; }

        // 아이콘, 모델, 레벨은 nullable 가능
        public int? IconId { get; set; }
        public int? ModelId { get; set; }

        [Range(1, 100, ErrorMessage = "레벨은 1~100 사이여야 합니다.")]
        public int? RequiredLevel { get; set; }

        // JSON 문자열 그대로 입력할지,
        // 아니면 나중에 복잡한 UI로 입력할지는 상황에 따라
        public string JsonStatModifiers { get; set; }
        public string JsonRequiredStats { get; set; }
        public string JsonUseableEffect { get; set; }
        public string SelectedIcon { get; set; }
    }
}
