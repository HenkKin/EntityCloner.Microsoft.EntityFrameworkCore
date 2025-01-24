using System;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.Extensions;
using Microsoft.EntityFrameworkCore;
using static EntityCloner.Microsoft.EntityFrameworkCore.Tests.CloneEnumerableOrderingIntegrationTests;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class TestDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleTranslation> ArticleTranslations { get; set; }

        public DbSet<Process> Processes { get; set; }
        public DbSet<Shape> Shapes { get; set; }

        public TestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Element>(entity =>
            //{
            //    entity.HasKey(e => e.Id);
            //    entity.Property(x => x.Id).ValueGeneratedOnAdd();
            //    entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            //});

            modelBuilder.Entity<Shape>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.HasMany(s => s.Predecessors);
            });

            modelBuilder.Entity<Process>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.HasMany(p => p.Shapes);
            });



            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
            modelBuilder.Entity<Customer>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Customer>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<Customer>().OwnsOne(x => x.Address, addressBuilder=>
            {
                addressBuilder.Property(x => x.CountryId).IsRequired();
                addressBuilder.HasOne(a => a.Country)
                    .WithMany()
                    .HasForeignKey(address => address.CountryId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            

            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<Order>().OwnsOne(x => x.InstallationAddress, addressBuilder =>
            {
                addressBuilder.Property(x => x.CountryId).IsRequired();
                addressBuilder.HasOne(a => a.Country)
                    .WithMany()
                    .HasForeignKey(address => address.CountryId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Order>().OwnsOne(x=>x.TotalOrderPrice, moneyBuilder =>
            {
                moneyBuilder.Property(e => e.Amount).IsRequired();
                moneyBuilder.Property(e => e.Currency).IsRequired();
            });

            modelBuilder.Entity<OrderLine>().HasKey(x => x.Id);
            modelBuilder.Entity<OrderLine>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderLine>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<OrderLine>().OwnsOne(x => x.UnitPrice, moneyBuilder =>
            {
                moneyBuilder.Property(e => e.Amount).IsRequired();
                moneyBuilder.Property(e => e.Currency).IsRequired();
            });
            modelBuilder.Entity<Article>().HasKey(x => x.Id);
            modelBuilder.Entity<Article>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Article>().Property(x => x.RowVersion).IsRowVersion();

            modelBuilder.Entity<ArticleTranslation>().HasKey(x => new { x.ArticleId, x.LocaleId });
            modelBuilder.Entity<ArticleTranslation>().Property(x => x.LocaleId).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);

            // These entities are dynamically added below
            //modelBuilder.Entity<Blog>();
            //modelBuilder.Entity<BlogAssets>();
            //modelBuilder.Entity<Post>();
            //modelBuilder.Entity<Tag>();
            //modelBuilder.Entity<TagHeader>();
            //modelBuilder.Entity<TagIpAddress>();
            var entitiesAssembly = typeof(IEntity).Assembly;
            modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);
        }
    }
}