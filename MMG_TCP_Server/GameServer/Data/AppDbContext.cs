using GameServer.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<CharacterDto> Characters { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 실제 연결 문자열로 교체해줘
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-ON7T0LI\SQLEXPRESS;Database=MMG_DB;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CharacterDto>()
                .ToTable("Character")
                .HasKey(p => p.Id);
        }
    }
}
