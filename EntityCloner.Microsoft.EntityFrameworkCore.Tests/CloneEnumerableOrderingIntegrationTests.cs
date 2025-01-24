using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class CloneEnumerableOrderingIntegrationTests : DbContextTestBase
    {
        private readonly DateTime _birthDate = new DateTime(1980, 01, 01);
        private readonly DateTime _offerDate = new DateTime(2020, 2, 20);
        private readonly DateTime _orderDate = new DateTime(2020, 3, 31);
        private readonly string _localeEnGb = "en-GB";
        private readonly string _localeNlNL = "nl-NL";

        private readonly Process _process;

        public CloneEnumerableOrderingIntegrationTests() : base(nameof(CloneEnumerableOrderingIntegrationTests))
        {
            _process = new Process
            {
                Id = 1,
                Title = "Process1",
            };

            var shape100 = new Shape { Id = 100, Title = "Shape100" };
            var shape200 = new Shape { Id = 200, Title = "Shape200" };
            var shape300 = new Shape { Id = 300, Title = "Shape300" };
            var shape400 = new Shape { Id = 400, Title = "Shape400" };
            var shape500 = new Shape { Id = 500, Title = "Shape500" };
            var shape600 = new Shape { Id = 600, Title = "Shape600" };
            var shape700 = new Shape { Id = 700, Title = "Shape700" };
            var shape800 = new Shape { Id = 800, Title = "Shape800" };
            var shape900 = new Shape { Id = 900, Title = "Shape900" };
            var shape1000 = new Shape { Id = 1000, Title = "Shape1000" };

            _process.Shapes.Add(shape500);
            shape500.Predecessors.Add(shape100);
            shape500.Predecessors.Add(shape200);
            shape500.Predecessors.Add(shape300);
            shape500.Predecessors.Add(shape400);

            _process.Shapes.Add(shape1000);
            shape1000.Predecessors.Add(shape600);
            shape1000.Predecessors.Add(shape700);
            shape1000.Predecessors.Add(shape800);
            shape1000.Predecessors.Add(shape900);

            TestDbContext.Processes.Add(_process);
            TestDbContext.SaveChanges();
        }

        [Fact]
        public async Task Process_Array()
        {
            // Arrange
            Process[] entities = await TestDbContext.Set<Process>()
                .Include(x => x.Shapes)
                .ThenInclude(x => x.Predecessors)
                .Where(c => c.Id == _process.Id)
                .AsNoTracking()
                .ToArrayAsync();

            // Act
            var cloneList = await TestDbContext.CloneAsync(entities);

            // Assert
            Assert.Single((IEnumerable)cloneList);

            var clone = cloneList.FirstOrDefault();
            Assert.Equal(_process.Title, clone.Title);
            Assert.Equal(_process.Shapes.Count, clone.Shapes.Count);

            for (var i = 0; i < clone.Shapes.Count; i++)
            {
                Assert.Equal(_process.Shapes.ElementAt(i).Title, clone.Shapes.ElementAt(i).Title);


                Assert.Equal(_process.Shapes.ElementAt(i).Predecessors.Count, clone.Shapes.ElementAt(i).Predecessors.Count);

                for (var p = 0; p < clone.Shapes.ElementAt(i).Predecessors.Count; p++)
                {
                    Assert.Equal(_process.Shapes.ElementAt(i).Predecessors.ElementAt(p).Title, clone.Shapes.ElementAt(i).Predecessors.ElementAt(p).Title);
                }
            }
        }
    }
}
