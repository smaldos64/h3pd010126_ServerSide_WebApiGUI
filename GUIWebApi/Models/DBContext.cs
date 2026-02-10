using Microsoft.EntityFrameworkCore;

namespace GUIWebApi.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        public DbSet<Category1> Categories1 { get; set; }
        public DbSet<Product1> Products1 { get; set; }
        public DbSet<InventoryFile> InventoryFiles { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category1>(entity =>
            {
                entity.HasKey(e => e.Category1Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            });
                        
            modelBuilder.Entity<Product1>(entity =>
            {
                entity.HasKey(e => e.Product1Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(450);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(e => e.Category1).WithMany(c => c.Products1).HasForeignKey(e => e.Category1Id).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UserFile)
                .WithMany(i => i.Products1)
                .HasForeignKey(e => e.UserFileId)
                .OnDelete(DeleteBehavior.SetNull);
            });
                        
            modelBuilder.Entity<Product1>()
                .HasOne(p => p.Category1)
                .WithMany(c => c.Products1)
                .HasForeignKey(p => p.Category1Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}