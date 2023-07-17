namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;

public class TagHeader : IEntity
{
    public int Id { get; set; } // Primary key
    public string Header { get; set; }
}