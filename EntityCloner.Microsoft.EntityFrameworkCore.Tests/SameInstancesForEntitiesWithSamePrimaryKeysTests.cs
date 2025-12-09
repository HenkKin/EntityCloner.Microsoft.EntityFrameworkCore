using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    public class SameInstancesForEntitiesWithSamePrimaryKeysTests : DbContextTestBase
    {
        private readonly Blog _blog;
        private readonly Post _post1;
        private readonly Post _post2;
        private readonly Tag _tag1;
        private readonly Tag _tag2;
        private readonly Tag _tag3;
        private readonly TagHeader _tagHeader1;
        private readonly TagHeader _tagHeader2;

        public SameInstancesForEntitiesWithSamePrimaryKeysTests() : base(nameof(SameInstancesForEntitiesWithSamePrimaryKeysTests))
        {
            _blog = new Blog
            {
                //Id = 1,
                Name = "Blog1"
            };

            _tagHeader1 = new TagHeader
            {
                Header = "Tagheader1"
            };

            _tagHeader2 = new TagHeader
            {
                Header = "Tagheader2"
            };

            _tag1 = new Tag
            {
                //Id = 3,
                Text = "tag1",
                TagHeader = _tagHeader1,
                TagHeaderId = _tagHeader1.Id,
            };
            _tag2 = new Tag
            {
                //Id = 4,
                Text = "tag2",
                TagHeader = _tagHeader2,
                TagHeaderId = _tagHeader2.Id,
            };
            _tag3 = new Tag
            {
                //Id = 5,
                Text = "tag3",
                TagHeader = _tagHeader1,
                TagHeaderId = _tagHeader1.Id,
            };

            _post1 = new Post
            {
                //Id = 6,
                Content = "ContentPost1",
                Title = "TitlePost1",
                BlogId = _blog.Id,
                Blog = _blog,
                Tags = new List<Tag> { _tag1, _tag2, _tag3 }

            };
            _blog.Posts.Add(_post1);

            _post2 = new Post
            {
                //Id = 7,
                Content = "ContentPost1",
                Title = "TitlePost1",
                BlogId = _blog.Id,
                Blog = _blog,
                Tags = new List<Tag> { _tag1, _tag2 }

            };
            _tag1.Posts.Add(_post1);
            _tag1.Posts.Add(_post2);
            _tag2.Posts.Add(_post1);
            _tag2.Posts.Add(_post2);

            _blog.Posts.Add(_post1);
            _blog.Posts.Add(_post2);

            _blog.FirstTag = _tag1;

            TestDbContext.Set<Blog>().Add(_blog);
            TestDbContext.SaveChanges();
        }
        [Fact]
        public async Task Blog_CloneWithIncludeOnTagsAndFirstTagBothShouldHaveSameInstanceButAlsoTagHeaderAsDeepestIncludedEntity()
        {
            // Arrange
            var entity = await TestDbContext.Set<Blog>()
                .Include(b => b.FirstTag) //TagHeader not Included here
                .Include(b => b.Posts)
                .ThenInclude(b => b.Tags)
                .ThenInclude(b => b.TagHeader)// Included here
                .Where(c => c.Id == _blog.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var cloneBlog = await TestDbContext.CloneAsync(entity);

            var cloneFirstTag = cloneBlog.FirstTag;
            var cloneTag = cloneBlog.Posts.First().Tags.First();

            Assert.Same(cloneTag, cloneFirstTag);
            Assert.NotNull(cloneTag.TagHeader);
            Assert.NotNull(cloneFirstTag.TagHeader); // Should also have include TagHeader from other include
            Assert.Same(cloneTag.TagHeader, cloneFirstTag.TagHeader); // Both TagHeaders should be same instance

        }
    }
}

