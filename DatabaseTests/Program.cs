using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting SQL Server container...");
AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue9825", true);

var container = new ContainerBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SA_PASSWORD", "Your_password123")
    .WithPortBinding(14333, 1433)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(1433))
    .Build();

await container.StartAsync();

var connectionString =
    $"Server=localhost,14333;Database=EfCloneTest;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

Console.WriteLine("Running migrations...");

var options = new DbContextOptionsBuilder<TestDbContext>()
    .UseSqlServer(connectionString)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
    .LogTo(
        Console.WriteLine,
        new[]
        {
            DbLoggerCategory.Database.Command.Name,   // SQL statements
            DbLoggerCategory.Query.Name               // LINQ → SQL vertaling
        },
        LogLevel.Information
    )
    .Options;

using (var db = new TestDbContext(options))
{
    Task.Delay(2000).Wait();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.People.Add(new Person { Name = "Henk" });
    db.People.Add(new Person { Name = "Peter" });

    db.Cars.Add(new Car
    {
        Id1 = 10,
        Id2 = 20,
        Brand = "Volvo",
        Model = "XC90"
    });


    db.Cars.Add(new Car
    {
        Id1 = 20,
        Id2 = 30,
        Brand = "Toyota",
        Model = "Prius"
    });

    await db.SaveChangesAsync();
}

Console.WriteLine("Running clone test...");

await CloneTests.RunAsync(options);

Console.WriteLine("Done.");
