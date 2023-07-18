using System;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase
{
    public abstract class DbContextTestBase
    {
        protected IServiceProvider ServiceProvider;
        protected TestDbContext TestDbContext;

        protected DbContextTestBase(string testName)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContextPool<TestDbContext>(
                builder => builder
                    .UseInMemoryDatabase(testName).EnableSensitiveDataLogging().EnableDetailedErrors());

            ServiceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;
            TestDbContext = ServiceProvider.GetRequiredService<TestDbContext>();
        }
    }
}
