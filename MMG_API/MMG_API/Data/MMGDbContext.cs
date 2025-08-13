using Microsoft.EntityFrameworkCore;
using MMG_API.Controllers.Quest;
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
        public DbSet<NpcTemplateEntity> NpcTemplates { get; set; }
        public DbSet<NpcSpawnEntity> NpcSpawns { get; set; }
        public DbSet<NpcQuestLinkEntity> NpcQuestLinks { get; set; }
        public DbSet<QuestEntity> Quests { get; set; }
        public DbSet<QuestGoalEntity> QuestGoals { get; set; }
        public DbSet<QuestRewardEntity> QuestRewards { get; set; }
        public DbSet<UserQuestEntity> UserQuest { get; set; }

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
            modelBuilder.Entity<NpcTemplateEntity>().ToTable("NpcTemplate").HasKey(t => t.TemplateId);  // 정확히 DB 테이블명
            modelBuilder.Entity<NpcSpawnEntity>().ToTable("NpcSpawn").HasKey(s => s.SpawnId);  // 정확히 DB 테이블명
            modelBuilder.Entity<NpcQuestLinkEntity>().ToTable("NpcQuestLinkTable").HasKey(l => new { l.NpcTemplateId, l.QuestId });  // 정확히 DB 테이블명
            modelBuilder.Entity<QuestEntity>().ToTable("Quest");  // 정확히 DB 테이블명
            modelBuilder.Entity<QuestGoalEntity>(e =>
            {
                e.ToTable("QuestGoal"); // 정확한 DB 테이블 이름

                // 복합 키 설정
                e.HasKey(x => new { x.QuestId, x.GoalIndex });

                // GoalIndex는 우리가 직접 값 지정 (DB 자동 생성 X)
                e.Property(x => x.QuestId).ValueGeneratedNever();
                e.Property(x => x.GoalIndex).ValueGeneratedNever();
            });
            modelBuilder.Entity<QuestRewardEntity>().ToTable("QuestReward");  // 정확히 DB 테이블명

            modelBuilder.Entity<MonsterSkill>()
                .HasKey(ms => new { ms.MonsterId, ms.SkillId }); // 복합 키 지정
            modelBuilder.Entity<CharacterSkill>()
                .HasKey(ms => new { ms.CharacterId, ms.SkillId }); // 복합 키 지정

            ModelCreateing_UserQuest(modelBuilder);
        }
        private void ModelCreateing_UserQuest(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserQuestEntity>(b =>
            {
                b.ToTable("UserQuest", "dbo");

                // 1) 복합 PK
                b.HasKey(x => new { x.CharacterId, x.QuestId });

                // 2) 컬럼 매핑
                b.Property(x => x.Status)
                    .HasColumnType("tinyint"); // byte 매핑

                b.Property(x => x.Progress)
                    .HasColumnName("Progress")
                    .HasColumnType("nvarchar(max)");

                // 3) 시간 타입 (DateTimeOffset ↔ datetimeoffset)
                b.Property(x => x.StartedAt)
                    .HasColumnType("datetimeoffset(7)")
                    .HasDefaultValueSql("SYSUTCDATETIME()"); // INSERT 시 기본값

                b.Property(x => x.CompletedAt)
                    .HasColumnType("datetimeoffset(7)");

                b.Property(x => x.UpdatedAt)
                    .HasColumnType("datetimeoffset(7)")
                    .HasDefaultValueSql("SYSUTCDATETIME()"); // INSERT 시 기본값

                // 4) CHECK 제약 (1=ACTIVE,2=COMPLETED,3=FAILED,4=ABANDONED)
                b.HasCheckConstraint("CK_UserQuest_Status", "[Status] IN (1,2,3,4)");

                // (선택) JSON 유효성
                b.HasCheckConstraint("CK_UserQuest_ProgressJson",
                    "[Progress] IS NULL OR ISJSON([Progress]) = 1");

                // 5) 조회용 인덱스
                b.HasIndex(x => new { x.CharacterId, x.Status })
                 .HasDatabaseName("IX_UserQuest_CharStatus");
            });
        }
    }
}
