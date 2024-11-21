using Microsoft.EntityFrameworkCore;
using PhotoGallery.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Image> Images { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<ImageLabel> ImageLabels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
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

    base.OnModelCreating(modelBuilder);
}
}



