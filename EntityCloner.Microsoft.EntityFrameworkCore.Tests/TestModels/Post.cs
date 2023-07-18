using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Post : IEntity
    {
        // https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes

        public int Id { get; set; } // Primary key
        public string Title { get; set; }
        public string Content { get; set; }

        public int? BlogId { get; set; } // Foreign key
        public Blog Blog { get; set; } // Reference navigation

        public ICollection<Tag> Tags { get; set; } = new List<Tag>(); // Skip collection navigation
    }
}