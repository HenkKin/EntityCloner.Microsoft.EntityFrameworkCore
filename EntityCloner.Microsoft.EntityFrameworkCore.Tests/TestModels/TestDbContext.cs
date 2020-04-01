using Microsoft.EntityFrameworkCore;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class TestDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleTranslation> ArticleTranslations { get; set; }

        public TestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
            modelBuilder.Entity<Customer>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Customer>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<Customer>().OwnsOne(x => x.Address);

            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().Property(x => x.RowVersion).IsRowVersion();

            modelBuilder.Entity<OrderLine>().HasKey(x => x.Id);
            modelBuilder.Entity<OrderLine>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderLine>().Property(x => x.RowVersion).IsRowVersion();

            modelBuilder.Entity<Article>().HasKey(x => x.Id);
            modelBuilder.Entity<Article>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Article>().Property(x => x.RowVersion).IsRowVersion();

            modelBuilder.Entity<ArticleTranslation>().HasKey(x => new { x.ArticleId, x.LocaleId });
            modelBuilder.Entity<ArticleTranslation>().Property(x => x.LocaleId).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}