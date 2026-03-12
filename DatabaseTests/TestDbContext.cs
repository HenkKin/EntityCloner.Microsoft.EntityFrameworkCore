using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasKey(c => new { c.Id1, c.Id2 });
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<Car> Cars => Set<Car>();
}