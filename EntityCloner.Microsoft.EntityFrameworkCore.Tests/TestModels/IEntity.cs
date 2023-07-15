using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public interface IEntity
    {
        public int Id { get; set; }

    }

    public class DynamicEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<DynamicChildEntity> ChildEntities { get; set; } = new HashSet<DynamicChildEntity>();
    }

    public class DynamicChildEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<DynamicChildChildEntity> ChildEntities { get; set; } = new HashSet<DynamicChildChildEntity>();
    }

    public class DynamicChildChildEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes
    public class Blog
    {
        public int Id { get; set; } // Primary key
        public string Name { get; set; }

        public IList<Post> Posts { get; } = new List<Post>(); // Collection navigation
        public BlogAssets Assets { get; set; } // Reference navigation
    }

    public class BlogAssets
    {
        public int Id { get; set; } // Primary key
        public byte[] Banner { get; set; }

        public int? BlogId { get; set; } // Foreign key
        public Blog Blog { get; set; } // Reference navigation
    }

    public class Post
    {
        public int Id { get; set; } // Primary key
        public string Title { get; set; }
        public string Content { get; set; }

        public int? BlogId { get; set; } // Foreign key
        public Blog Blog { get; set; } // Reference navigation

        public IList<Tag> Tags { get; } = new List<Tag>(); // Skip collection navigation
    }

    public class Tag
    {
        public int Id { get; set; } // Primary key
        public string Text { get; set; }

        public IList<Post> Posts { get; } = new List<Post>(); // Skip collection navigation
    }
}

