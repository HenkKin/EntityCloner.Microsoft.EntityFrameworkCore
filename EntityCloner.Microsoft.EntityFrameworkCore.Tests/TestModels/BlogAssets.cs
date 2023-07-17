namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;

public class BlogAssets : IEntity
{
    // https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes

    public int Id { get; set; } // Primary key
    public byte[] Banner { get; set; }

    public int? BlogId { get; set; } // Foreign key
    public Blog Blog { get; set; } // Reference navigation
}