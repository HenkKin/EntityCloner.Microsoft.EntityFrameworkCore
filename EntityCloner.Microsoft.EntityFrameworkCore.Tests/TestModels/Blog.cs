using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Blog : IEntity
    {
        // https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes

        public int Id { get; set; } // Primary key
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>(); // Collection navigation
        public BlogAssets Assets { get; set; } // Reference navigation
    }
}