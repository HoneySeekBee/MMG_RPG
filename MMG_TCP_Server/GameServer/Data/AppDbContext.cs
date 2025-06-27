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
        public DbSet<PlacedObjectDTO> PlacedObjects { get; set; }
        public DbSet<PlantedCropDto> PlantedCrop { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 실제 연결 문자열로 교체해줘
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-ON7T0LI\SQLEXPRESS;Database=MMG_DB;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlacedObjectDTO>()
                .ToTable("PlacedObjects") // 테이블 이름 매핑
                .HasKey(p => p.Id);

            modelBuilder.Entity<PlacedObjectDTO>()
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<PlantedCropDto>()
                .ToTable("PlantedCrop")
                .HasKey(p => p.Id);

        }
    }
}
