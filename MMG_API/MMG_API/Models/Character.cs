using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MMG_API.Models
{
    [Table("Character")]
    public class Character
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SlotNumber { get; set; }
        [Required]
        public int Gender { get; set; }

        [Required]
        [MaxLength(50)]
        public string CharacterName { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Class { get; set; } = string.Empty;

        [Required]
        public string AppearanceCode { get; set; } = string.Empty;

        public int? LastMapId { get; set; }
        public int? LastSpawnPointId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastPlayedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
