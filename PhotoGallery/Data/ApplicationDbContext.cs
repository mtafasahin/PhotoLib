using Microsoft.EntityFrameworkCore;
using PhotoGallery.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Image> Images { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<ImageLabel> ImageLabels { get; set; }
    public DbSet<ImageSimilarity> ImageSimilarities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{

    // ImageSimilarity tablosundaki ilişkiler
        modelBuilder.Entity<ImageSimilarity>()
            .HasOne(s => s.Image)
            .WithMany()
            .HasForeignKey(s => s.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Çoka Çoka ilişki: Image <-> Label
        modelBuilder.Entity<ImageLabel>()
            .HasKey(il => new { il.ImageId, il.LabelId });

        modelBuilder.Entity<ImageLabel>()
            .HasOne(il => il.Image)
            .WithMany(i => i.ImageLabels)
            .HasForeignKey(il => il.ImageId);

        modelBuilder.Entity<ImageLabel>()
            .HasOne(il => il.Label)
            .WithMany(l => l.ImageLabels)
            .HasForeignKey(il => il.LabelId);

        // Global Query Filter for Soft Delete
        modelBuilder.Entity<Image>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Label>().HasQueryFilter(i => !i.IsDeleted);

        base.OnModelCreating(modelBuilder);
}
}



