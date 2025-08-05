using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMG_API.Models
{
    [Table("QuestReward")]
    public class QuestRewardEntity
    {
        [Key]
        public int QuestRewardId { get; set; } // PK
        public int QuestId { get; set; }
        public int RewardType { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Exp { get; set; }
    }
}
