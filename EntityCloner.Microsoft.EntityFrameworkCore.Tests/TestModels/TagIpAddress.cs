namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;

public class TagIpAddress : IEntity
{
    public int Id { get; set; } // Primary key
    public string IpAddress { get; set; }
    public int TagId { get; set; } // Foreign key
    public Tag Tag { get; set; } // Reference navigation
}