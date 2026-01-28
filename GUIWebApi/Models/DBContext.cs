using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ImageFile> ImageFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(450);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ImageFile)
                .WithMany(i => i.Products)
                .HasForeignKey(e => e.ImageFileId)
                .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ImageFile>(entity =>
            {
                entity.HasKey(e => e.ImageFileId);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
                entity.Property(e => e.RelativePath).IsRequired().HasMaxLength(1024);

                entity.HasIndex(e => e.RelativePath).IsUnique();
            });
        }
    }
}