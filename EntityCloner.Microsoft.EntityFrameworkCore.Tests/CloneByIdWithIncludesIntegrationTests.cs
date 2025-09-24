using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    public class CloneByIdWithIncludesIntegrationTests : DbContextTestBase
    {
        private readonly DateTime _birthDate = new DateTime(1980, 01, 01);
        private readonly DateTime _offerDate = new DateTime(2020, 2, 20);
        private readonly DateTime _orderDate = new DateTime(2020, 3, 31);
        private readonly string _localeEnGb = "en-GB";
        private readonly string _localeNlNL = "nl-NL";

        private readonly Customer _customer;

        public CloneByIdWithIncludesIntegrationTests() : base(nameof(CloneByIdWithIncludesIntegrationTests))
        {
            var country = new Country
            {
                Name = "Netherlands"
            };
            _customer = new Customer
            {
                RowVersion = new[] { byte.MinValue },
                BirthDate = _birthDate,
                Address = new Address
                {
                    HouseNumber = 25,
                    Street = "Street",
                    Country = country
                },
                Orders = new List<Order>
                {
                    new Order
                    {
                        RowVersion = new[] { byte.MinValue },
                        Customer = new Customer(),
                        Description = "Description",
                        IsDeleted = false,
                        OfferDate = _offerDate,
                        OrderDate = _orderDate,
                        OrderStatus = OrderStatus.Order,
                        TenantId = 1,
                        InstallationAddress = new Address
                        {
                            HouseNumber = 25,
                            Street = "Street",
                            Country = country
                        },
                        TotalOrderPrice = new Money{ Amount = 1000m, Currency = "EUR"},
                        OrderLines = new List<OrderLine>
                        {
                            new OrderLine
                            {
                                Quantity = 1,
                                UnitPrice  = new Money{ Amount = 500m, Currency = "EUR"},
                                Article = new Article
                                {

                                    ArticleTranslations = new List<ArticleTranslation>
                                    {
                                        new ArticleTranslation
                                        {
                                            Description = "Artikel 1 en-GB",
                                            LocaleId = _localeEnGb
                                        },
                                        new ArticleTranslation
                                        {
                                            Description = "Artikel 1 nl-NL",
                                            LocaleId = _localeNlNL
                                        }
                                    }
                                }
                            },
                            new OrderLine
                            {
                                Quantity = 2,
                                UnitPrice  = new Money{ Amount = 250m, Currency = "EUR"},
                                Article = new Article
                                {
                                    ArticleTranslations = new List<ArticleTranslation>
                                    {
                                        new ArticleTranslation
                                        {
                                            Description = "Artikel 2 en-GB",
                                            LocaleId = _localeEnGb
                                        },
                                        new ArticleTranslation
                                        {
                                            Description = "Artikel 2 nl-NL",
                                            LocaleId = _localeNlNL
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            TestDbContext.Customers.Add(_customer);
            TestDbContext.SaveChanges();
        }

        [Fact]
        public async Task Customer_IncludeEntityWithoutIncludeForOwnsEntityAddress()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                , _customer.Id);

            // Assert
            Assert.NotNull(clone.Address);
        }

        [Fact]
        public async Task Customer_IncludeEntityWithIncludeForOwnsEntityAddress()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Address)
                    //.ThenInclude(c => c.Country)
                    .Include(c => c.Orders)
                    .ThenInclude(c => c.InstallationAddress)
                    //.ThenInclude(c => c.Country)
                , _customer.Id);

            // Assert
            Assert.NotNull(clone.Address);
        }

        [Fact]
        public async Task Customer_IncludeEntityWithoutOrderLines()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Orders)
                , _customer.Id);

            // Assert
            Assert.Empty(clone.Orders.SelectMany(o => o.OrderLines));
        }

        [Fact]
        public async Task Customer_IncludeEntityWithOrderLines()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Orders)
                    .ThenInclude(c => c.OrderLines)
                , _customer.Id);

            // Assert
            Assert.Equal(2, clone.Orders.SelectMany(o => o.OrderLines).Count());
        }

        [Fact]
        public async Task Customer_IncludeEntityWithoutArticleTranslations()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Orders)
                    .ThenInclude(c => c.OrderLines)
                    .ThenInclude(c => c.Article)
                , _customer.Id);

            // Assert
            Assert.Empty(clone.Orders.SelectMany(o => o.OrderLines.SelectMany(ol => ol.Article.ArticleTranslations)));
        }

        [Fact]
        public async Task Customer_IncludeEntityWithArticleTranslations()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Orders)
                    .ThenInclude(c => c.OrderLines)
                    .ThenInclude(c => c.Article)
                    .ThenInclude(c => c.ArticleTranslations)
                , _customer.Id);

            // Assert
            Assert.Equal(4, clone.Orders.SelectMany(o => o.OrderLines.SelectMany(ol => ol.Article.ArticleTranslations)).Count());
        }

        [Fact]
        public async Task Customer_IncludeEntityWithAllPossibleIncludes()
        {
            // Act
            var clone = await TestDbContext.CloneAsync<Customer>(x => x
                    .Include(c => c.Orders)
                    .ThenInclude(c => c.OrderLines)
                    .ThenInclude(c => c.Article)
                    .ThenInclude(c => c.ArticleTranslations)
                    .Include(c => c.Orders)
                    .Include(c => c.Address)
                , _customer.Id);

            // Assert

            // Customer
            Assert.Equal(0, clone.Id);
            Assert.Equal(_birthDate, clone.BirthDate);
            Assert.NotNull(clone.Address);
            Assert.Equal(25, clone.Address.HouseNumber);
            Assert.Equal("Street", clone.Address.Street);
            Assert.Equal(1, clone.Orders.Count);

            // Order
            var order = clone.Orders.Single();
            Assert.Equal(0, clone.Id);
            Assert.NotNull(order.Customer);
            Assert.Equal(0, order.CustomerId);
            Assert.Equal("Description", order.Description);
            Assert.Equal(OrderStatus.Order, order.OrderStatus);
            Assert.False(order.IsDeleted);
            Assert.Equal(_offerDate, order.OfferDate);
            Assert.Equal(_orderDate, order.OrderDate);
            Assert.Equal(2, order.OrderLines.Count);

            // OrderLine 1
            var orderLine1 = order.OrderLines.First();

            Assert.Equal(0, orderLine1.Id);
            Assert.NotNull(orderLine1.Order);
            Assert.Equal(0, orderLine1.OrderId);
            Assert.NotNull(orderLine1.Article);
            Assert.Equal(0, orderLine1.ArticleId);
            Assert.Equal(1, orderLine1.Quantity);

            // Article
            Assert.Equal(0, orderLine1.Article.Id);
            Assert.Equal(2, orderLine1.Article.ArticleTranslations.Count);

            // ArticleTranslations1
            var orderLine1Article1ArticleTranslations1 = orderLine1.Article.ArticleTranslations.First();
            Assert.Null(orderLine1Article1ArticleTranslations1.LocaleId); // is part of PrimaryKey
            Assert.Equal("Artikel 1 en-GB", orderLine1Article1ArticleTranslations1.Description);
            Assert.Equal(0, orderLine1Article1ArticleTranslations1.ArticleId);

            // ArticleTranslations2
            var orderLine1Article1ArticleTranslations2 = orderLine1.Article.ArticleTranslations.Last();
            Assert.Null(orderLine1Article1ArticleTranslations2.LocaleId);// is part of PrimaryKey
            Assert.Equal("Artikel 1 nl-NL", orderLine1Article1ArticleTranslations2.Description);
            Assert.Equal(0, orderLine1Article1ArticleTranslations2.ArticleId);

            // OrderLine 2
            var orderLine2 = order.OrderLines.Last();

            Assert.Equal(0, orderLine2.Id);
            Assert.NotNull(orderLine2.Order);
            Assert.Equal(0, orderLine2.OrderId);
            Assert.NotNull(orderLine2.Article);
            Assert.Equal(0, orderLine2.ArticleId);
            Assert.Equal(2, orderLine2.Quantity);

            // Article
            Assert.Equal(0, orderLine2.Article.Id);
            Assert.Equal(2, orderLine2.Article.ArticleTranslations.Count);

            // ArticleTranslations1
            var orderLine2Article1ArticleTranslations1 = orderLine2.Article.ArticleTranslations.First();
            Assert.Null(orderLine2Article1ArticleTranslations1.LocaleId);// is part of PrimaryKey
            Assert.Equal("Artikel 2 en-GB", orderLine2Article1ArticleTranslations1.Description);
            Assert.Equal(0, orderLine2Article1ArticleTranslations1.ArticleId);

            // ArticleTranslations2
            var orderLine2Article1ArticleTranslations2 = orderLine2.Article.ArticleTranslations.Last();
            Assert.Null(orderLine2Article1ArticleTranslations2.LocaleId);// is part of PrimaryKey
            Assert.Equal("Artikel 2 nl-NL", orderLine2Article1ArticleTranslations2.Description);
            Assert.Equal(0, orderLine2Article1ArticleTranslations2.ArticleId);
        }


        [Fact]
        public async Task Customer_IncludeEntityWithCompositeKeyAndIgnorePartOfCompositeKey()
        {
            // Act
            int articleId = 1;
            var clone = await TestDbContext.CloneAsync<Article>(x => x
                    //.Include(c => c.ArticleTranslations, c => c.Select(x => x.LocaleId)).Skip(x => x.Select(x => x.LocaleId)).Skip(x => x.Select(x => x.)).ThenInclude()
                    .IncludeSkip(c => c.ArticleTranslations, x => x
                        .SkipProperty(a => a.Select(x => x.LocaleId))
                        .SkipProperty(a => a.Select(x => x.ArticleId)))
                    // .SkipProperty(x => x.Select(x => x.LocaleId))
                    // .SkipProperty(x => x.Select(x => x.ArticleId))
                    //.ThenIncludeSkip(x => x.Article, x => x.SkipProperty(s => s.Id).SkipProperty(s => s.Id))
                , articleId);

            var clone2 = await TestDbContext.CloneAsync<Article>(x => x
                    .Include(c => c.ArticleTranslations)
                     .SkipProperty(x => x.Select(x => x.LocaleId))
                     .SkipProperty(x => x.Select(x => x.ArticleId))
                    //.ThenInclude(x => x.Article)
                , articleId);

            var clone3 = await TestDbContext.CloneAsync<Article>(x => x
                    .Include(c => c.ArticleTranslations, x => x.Select(a => a.LocaleId), x => x.Select(a => a.ArticleId))
                    //.ThenInclude(x => x.Article)
                , articleId);

            // Assert

            // Article
            Assert.Equal(0, clone.Id);
            Assert.Equal(2, clone.ArticleTranslations.Count);
            Assert.Empty(clone.OrderLines);


            // ArticleTranslations1
            var orderLine1Article1ArticleTranslations1 = clone.ArticleTranslations.First();
            Assert.Null(orderLine1Article1ArticleTranslations1.LocaleId); // is part of PrimaryKey
            Assert.Equal("Artikel 1 en-GB", orderLine1Article1ArticleTranslations1.Description);
            Assert.Equal(0, orderLine1Article1ArticleTranslations1.ArticleId);

            // ArticleTranslations2
            var orderLine1Article1ArticleTranslations2 = clone.ArticleTranslations.Last();
            Assert.Null(orderLine1Article1ArticleTranslations2.LocaleId);// is part of PrimaryKey
            Assert.Equal("Artikel 1 nl-NL", orderLine1Article1ArticleTranslations2.Description);
            Assert.Equal(0, orderLine1Article1ArticleTranslations2.ArticleId);
        }
    }
}
