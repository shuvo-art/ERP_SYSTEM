using Microsoft.EntityFrameworkCore;
using ReferenceProjectApi.Core.Entities;

namespace ReferenceProjectApi.Infrastructure.Data;

public class ReferenceProjectDbContext : DbContext
{
    public ReferenceProjectDbContext(DbContextOptions<ReferenceProjectDbContext> options) : base(options)
    {
    }

    public DbSet<ProjectCategory> ProjectCategories { get; set; }
    public DbSet<ReferenceProject> ReferenceProjects { get; set; }
    public DbSet<ProjectGalleryImage> ProjectGalleryImages { get; set; }
    public DbSet<ProjectDetailImage> ProjectDetailImages { get; set; }
    public DbSet<ProjectProductJunction> ProjectProducts { get; set; }
    public DbSet<Product> Products { get; set; } // Mapping to existing products table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProjectCategory Configuration
        modelBuilder.Entity<ProjectCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
        });

        // ReferenceProject Configuration
        modelBuilder.Entity<ReferenceProject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProjectName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("ongoing");
            
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectGalleryImage Configuration
        modelBuilder.Entity<ProjectGalleryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Project)
                .WithMany(p => p.GalleryImages)
                .HasForeignKey(d => d.ProjectId);
        });

        // ProjectDetailImage Configuration
        modelBuilder.Entity<ProjectDetailImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Project)
                .WithMany(p => p.DetailImages)
                .HasForeignKey(d => d.ProjectId);
        });

        // Many-to-Many Configuration for Project-Product
        modelBuilder.Entity<ProjectProductJunction>(entity =>
        {
            entity.HasKey(e => new { e.ProjectId, e.ProductId });

            entity.HasOne(d => d.Project)
                .WithMany(p => p.ProjectProducts)
                .HasForeignKey(d => d.ProjectId);

            entity.HasOne(d => d.Product)
                .WithMany(p => p.ProjectProducts)
                .HasForeignKey(d => d.ProductId);
        });

        // Map to existing products table (assume table name is "Products")
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products"); // Adjust table name if different
            entity.HasKey(e => e.Id);
        });
    }
}
