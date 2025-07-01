using Microsoft.EntityFrameworkCore;
using MMG_API.Models;

namespace MMG_API.Data
{
    public class MMGDbContext: DbContext
    {
        public MMGDbContext(DbContextOptions<MMGDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // 여기에 추가해줘야 DB랑 연결됨
        public DbSet<Character> Characters { get; set; } // 여기에 추가해줘야 DB랑 연결됨

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User"); // 정확히 DB 테이블명
            modelBuilder.Entity<Character>().ToTable("Character"); // 정확히 DB 테이블명
        }

    }
}
