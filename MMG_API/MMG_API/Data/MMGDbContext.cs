using Microsoft.EntityFrameworkCore;
using MMG_API.Models;

namespace MMG_API.Data
{
    public class MMGDbContext: DbContext
    {
        public MMGDbContext(DbContextOptions<MMGDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<Character> Characters { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<CharacterStatus> CharacterStatuses { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<Skill> Skills { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<CharacterSkill> CharacterSkills { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<Monster> Monsters { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<MonsterSkill> MonsterSkills { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<ItemEntity> Items { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User"); // 정확히 DB 테이블명
            modelBuilder.Entity<Character>().ToTable("Character"); // 정확히 DB 테이블명
            modelBuilder.Entity<CharacterStatus>().ToTable("CharacterStatus"); // 정확히 DB 테이블명
            modelBuilder.Entity<Skill>().ToTable("Skill"); // 정확히 DB 테이블명
            modelBuilder.Entity<CharacterSkill>().ToTable("CharacterSkill"); // 정확히 DB 테이블명
            modelBuilder.Entity<Monster>().ToTable("Monster"); // 정확히 DB 테이블명
            modelBuilder.Entity<MonsterSkill>().ToTable("MonsterSkill"); // 정확히 DB 테이블명
            modelBuilder.Entity<ItemEntity>().ToTable("ItemData").HasKey(i => i.ItemId);  // 정확히 DB 테이블명

            modelBuilder.Entity<MonsterSkill>()
                .HasKey(ms => new { ms.MonsterId, ms.SkillId }); // 복합 키 지정
            modelBuilder.Entity<CharacterSkill>()
                .HasKey(ms => new { ms.CharacterId, ms.SkillId }); // 복합 키 지정
        }
    }
}
