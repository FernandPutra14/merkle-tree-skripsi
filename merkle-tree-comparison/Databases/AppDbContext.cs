using merkle_tree_comparison.Databases.Models;
using Microsoft.EntityFrameworkCore;

namespace merkle_tree_comparison.Databases;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasKey(e => e.Id);
        modelBuilder.Entity<User>().HasMany(e => e.UserFiles).WithOne(u => u.User);

        modelBuilder.Entity<UserFile>().HasKey(e => e.Id);
        modelBuilder.Entity<UserFile>().HasOne(e => e.User).WithMany(u => u.UserFiles);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserFile> UserFiles { get; set; }
}
