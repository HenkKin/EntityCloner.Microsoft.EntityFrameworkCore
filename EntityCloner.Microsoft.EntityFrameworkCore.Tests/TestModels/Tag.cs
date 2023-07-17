using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;

public class Tag : IEntity
{
    // https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes

    public int Id { get; set; } // Primary key
    public string Text { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>(); // Skip collection navigation
    public ICollection<TagIpAddress> TagIpAddresses { get; set; } = new List<TagIpAddress>(); // Collection navigation

    public int TagHeaderId { get; set; } // Foreign key
    public TagHeader TagHeader { get; set; } // Reference navigation
}