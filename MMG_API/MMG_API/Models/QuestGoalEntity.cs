using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMG_API.Models
{
    [Table("QuestGoal")]
    public class QuestGoalEntity
    {
        public int GoalIndex { get; set; }

        public int QuestId { get; set; }

        public int GoalType { get; set; }

        public int TargetId { get; set; }

        public int Count { get; set; }
    }
}
