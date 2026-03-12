using EntityCloner.Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public static class CloneTests
{
    public static async Task RunAsync(DbContextOptions<TestDbContext> options)
    {
        using var db = new TestDbContext(options);


        var cloned = await db.CloneAsync<Person>(1);

        Console.WriteLine($"Cloned person: {cloned.Id} - {cloned.Name}");

        var clonedCar = await db.CloneAsync<Car>(new object[] { 10, 20 });
        Console.WriteLine($"Cloned car: {clonedCar.Id1}:{clonedCar.Id2} - {clonedCar.Brand} {clonedCar.Model}");
    }
}