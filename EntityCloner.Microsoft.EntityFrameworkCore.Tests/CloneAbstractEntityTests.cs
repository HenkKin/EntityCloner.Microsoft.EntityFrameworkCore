using System;
using System.Linq;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    public class CloneAbstractEntityTests : DbContextTestBase
    {
        private readonly Order _order;
        private readonly Payment _creditCardPayment;
        private readonly Payment _bankTransferPayment;

        public CloneAbstractEntityTests() : base(nameof(CloneAbstractEntityTests))
        {
            _creditCardPayment = new CreditCardPayment
            {
                Amount = 150.00m,
                Date = DateTime.Now,
                CardNumber = "1234-5678-9012-3456",
                CardHolder = "John Doe"
            };

            _bankTransferPayment = new BankTransferPayment
            {
                Amount = 300.00m,
                Date = DateTime.Now,
                Iban = "IT60X0542811101000000123456",
                BankName = "Test Bank"
            };

            _order = new Order
            {
                Description = "ORD-001",
                Payment = _creditCardPayment
            };

            TestDbContext.Set<Order>().Add(_order);
            TestDbContext.SaveChanges();
        }

        [Fact]
        public async Task Order_CloneWithoutIncludePayment()
        {
            // Arrange
            var entity = await TestDbContext.Set<Order>()
                .Where(o => o.Id == _order.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clone = await TestDbContext.CloneAsync(entity);

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(0, clone.Id);
            Assert.Equal(_order.Description, clone.Description);
            Assert.Null(clone.Payment);
        }

        [Fact]
        public async Task Order_CloneWithIncludeAbstractPayment()
        {
            // Arrange
            var entity = await TestDbContext.Set<Order>()
                .Include(o => o.Payment)
                .Where(o => o.Id == _order.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clone = await TestDbContext.CloneAsync(entity);

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(0, clone.Id);
            Assert.Equal(_order.Description, clone.Description);

            // Verify payment is cloned and type is preserved
            Assert.NotNull(clone.Payment);
            Assert.Equal(0, clone.Payment.Id);
            Assert.IsType<CreditCardPayment>(clone.Payment);

            var clonedCreditCard = (CreditCardPayment)clone.Payment;
            Assert.Equal("1234-5678-9012-3456", clonedCreditCard.CardNumber);
            Assert.Equal("John Doe", clonedCreditCard.CardHolder);
            Assert.Equal(150.00m, clonedCreditCard.Amount);
        }

        [Fact]
        public async Task Order_CloneWithDifferentConcreteImplementation()
        {
            // Arrange - Create order with bank transfer
            var orderWithBankTransfer = new Order
            {
                Description = "ORD-002",
                Payment = _bankTransferPayment
            };

            TestDbContext.Set<Order>().Add(orderWithBankTransfer);
            TestDbContext.SaveChanges();

            var entity = await TestDbContext.Set<Order>()
                .Include(o => o.Payment)
                .Where(o => o.Id == orderWithBankTransfer.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clone = await TestDbContext.CloneAsync(entity);

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(0, clone.Id);
            Assert.NotNull(clone.Payment);
            Assert.Equal(0, clone.Payment.Id);
            Assert.IsType<BankTransferPayment>(clone.Payment);

            var clonedBankTransfer = (BankTransferPayment)clone.Payment;
            Assert.Equal("IT60X0542811101000000123456", clonedBankTransfer.Iban);
            Assert.Equal("Test Bank", clonedBankTransfer.BankName);
            Assert.Equal(300.00m, clonedBankTransfer.Amount);
        }
    }
}