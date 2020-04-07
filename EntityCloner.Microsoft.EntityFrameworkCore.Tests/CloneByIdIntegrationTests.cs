using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    public class CloneByIdIntegrationTests : DbContextTestBase
    {
        public CloneByIdIntegrationTests() : base(nameof(CloneByIdIntegrationTests))
        {
        }

        [Fact]
        public async Task Customer_CloneSingleEntity()
        {
            // Arrange
            var birthDate = new DateTime(1980,01,01);
            var subject = new Customer
            {
                RowVersion = new[] {byte.MinValue},
                BirthDate = birthDate,
                Address = new Address
                {
                    HouseNumber = 25,
                    Street = "Street"
                },
                Orders = new List<Order>
                {
                    new Order()
                }
            };

            TestDbContext.Customers.Add(subject);
            await TestDbContext.SaveChangesAsync();

            Assert.NotEqual(0, subject.Id);
            Assert.Equal(birthDate, subject.BirthDate);
            Assert.NotNull(subject.Address);
            Assert.Equal(25, subject.Address.HouseNumber);
            Assert.Equal("Street", subject.Address.Street);
            Assert.Equal(1, subject.Orders.Count);
            Assert.NotEqual(0, subject.Orders.First().Id);

            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(subject.Id);

            // Assert
            Assert.Equal(0, clone.Id);
            Assert.Equal(birthDate, clone.BirthDate);
            Assert.NotNull(clone.Address);
            Assert.Equal(25, clone.Address.HouseNumber);
            Assert.Equal("Street", clone.Address.Street);
            Assert.Empty(clone.Orders);
        }


        [Fact]
        public async Task Order_CloneSingleEntity()
        {
            // Arrange
            var offerDate = new DateTime(2020, 2, 20);
            var orderDate = new DateTime(2020, 3, 31);
            var subject = new Order
            {
                RowVersion = new[] { byte.MinValue },
                Customer = new Customer(),
                Description = "Description",
                IsDeleted = false,
                OfferDate = offerDate,
                OrderDate = orderDate,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine()
                },
                OrderStatus = OrderStatus.Order,
                TenantId = 1
            };

            TestDbContext.Orders.Add(subject);
            await TestDbContext.SaveChangesAsync();

            Assert.NotEqual(0, subject.Id);
            Assert.NotNull(subject.Customer);
            Assert.Equal(1, subject.CustomerId);
            Assert.Equal(1, subject.Customer.Id);
            Assert.Equal("Description", subject.Description);
            Assert.False(subject.IsDeleted);
            Assert.Equal(offerDate, subject.OfferDate);
            Assert.Equal(orderDate, subject.OrderDate);
            Assert.Equal(1, subject.OrderLines.Count);
            Assert.Equal(1, subject.OrderLines.First().Id);
            Assert.Equal(1, subject.TenantId);

            // Act
            var clone = await TestDbContext.CloneAsync<Order>(subject.Id);

            // Assert
            Assert.Equal(0, clone.Id);
            Assert.Null(clone.Customer);
            Assert.Equal(1, clone.CustomerId);
            Assert.Equal("Description", clone.Description);
            Assert.False(clone.IsDeleted);
            Assert.Equal(offerDate, clone.OfferDate);
            Assert.Equal(orderDate, clone.OrderDate);
            Assert.Empty(clone.OrderLines);
        }

        [Fact]
        public async Task ArticleTranslation_CloneSingleEntityWithPrimaryKeyWithMultipleProperties()
        {
            // Arrange
            var article = new Article();

            var subject = new ArticleTranslation
            {
                ArticleId = 0,
                LocaleId = "en-GB",
                Description = "Description",
                Article = article,
            };

            TestDbContext.ArticleTranslations.Add(subject);

            await TestDbContext.SaveChangesAsync();

            Assert.NotEqual(0, subject.ArticleId);
            Assert.Equal("en-GB", subject.LocaleId);
            Assert.NotNull(subject.Article);
            Assert.Equal("Description", subject.Description);

            // Act
            var clone = await TestDbContext.CloneAsync<ArticleTranslation>(subject.ArticleId, subject.LocaleId);

            // Assert
            Assert.Equal(0, clone.ArticleId);
            Assert.Null(clone.LocaleId);
            Assert.Null(clone.Article);
            Assert.Equal("Description", clone.Description);
        }
    }
}
