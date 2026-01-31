using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestBase;
using EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests
{
    public class CloneRelationshipsTests : DbContextTestBase
    {
        private readonly Blog _blog;
        private readonly BlogAssets _blogAssets;
        private readonly Post _post1;
        private readonly Post _post2;
        private readonly Tag _tag1;
        private readonly Tag _tag2;
        private readonly Tag _tag3;
        private readonly TagHeader _tagHeader1;
        private readonly TagHeader _tagHeader2;
        private readonly TagIpAddress _tagIpAddress1;
        private readonly TagIpAddress _tagIpAddress2;
        private readonly TagIpAddress _tagIpAddress3;

        public CloneRelationshipsTests() : base(nameof(CloneRelationshipsTests))
        {
            _blog = new Blog
            {
                //Id = 1,
                Name = "Blog1"
            };

            _blogAssets = new BlogAssets
            {
                //Id = 2, 
                Banner = Encoding.UTF8.GetBytes("Dit is een test"),
                BlogId = _blog.Id,
                Blog = _blog
            };
            _blog.Assets = _blogAssets;

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


            _tagIpAddress1 = new TagIpAddress { IpAddress = "IpAddress1", Tag = _tag1, TagId = _tag1.Id };
            _tagIpAddress2 = new TagIpAddress { IpAddress = "IpAddress2", Tag = _tag1, TagId = _tag1.Id };
            _tagIpAddress3 = new TagIpAddress { IpAddress = "IpAddress3", Tag = _tag2, TagId = _tag2.Id };

            _tag1.TagIpAddresses.Add(_tagIpAddress1);
            _tag1.TagIpAddresses.Add(_tagIpAddress2);
            _tag2.TagIpAddresses.Add(_tagIpAddress3);

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


            TestDbContext.Set<Blog>().Add(_blog);
            TestDbContext.SaveChanges();
        }

        [Fact]
        public async Task Blog_CloneWithoutIncludes()
        {
            // Arrange
            var entity = await TestDbContext.Set<Blog>()
                .Where(c => c.Id == _blog.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clone = await TestDbContext.CloneAsync(entity);

            // Assert
            Assert.NotNull(clone);
            Assert.Equal(0, clone.Id);
            Assert.Equal(entity.Name, clone.Name);
            Assert.Null(clone.Assets);
            Assert.Empty(clone.Posts);
        }


        [Fact]
        public async Task Blog_CloneWithIncludeOfOptionalOneToManyReferenceRelation()
        {
            // Arrange
            var entity = await TestDbContext.Set<Blog>()
                .Include(b => b.Posts)
                .Where(c => c.Id == _blog.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var cloneBlog = await TestDbContext.CloneAsync(entity);

            // Assert
            Assert.NotNull(cloneBlog);
            Assert.Null(cloneBlog.Assets);
            Assert.NotEmpty(cloneBlog.Posts);
            Assert.Equal(2, cloneBlog.Posts.Count);

            var clonePost1 = cloneBlog.Posts.ElementAt(0);
            Assert.Equal(0, clonePost1.Id);
            Assert.Same(cloneBlog, clonePost1.Blog);
            Assert.Null(clonePost1.BlogId);
            Assert.Equal(_post1.Content, clonePost1.Content);
            Assert.Equal(_post1.Title, clonePost1.Title);
            Assert.Empty(clonePost1.Tags);

            var clonePost2 = cloneBlog.Posts.ElementAt(1);
            Assert.Equal(0, clonePost2.Id);
            Assert.Same(cloneBlog, clonePost2.Blog);
            Assert.Null(clonePost2.BlogId);
            Assert.Equal(_post1.Content, clonePost2.Content);
            Assert.Equal(_post1.Title, clonePost2.Title);
            Assert.Empty(clonePost2.Tags);
        }


        [Fact]
        public async Task Post_CloneWithIncludeOfManyToManyReferenceRelation()
        {
            // Arrange
            var entity = await TestDbContext.Set<Post>()
                .Include(b => b.Tags)
                .Where(c => c.Id == _post1.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clonePost = await TestDbContext.CloneAsync(entity);
            // Assert
            Assert.NotNull(clonePost);
            Assert.Null(clonePost.Blog);
            Assert.NotEmpty(clonePost.Tags);
            Assert.Equal(3, clonePost.Tags.Count);

            var cloneTag1 = clonePost.Tags.ElementAt(0);
            Assert.Equal(0, cloneTag1.Id);
            Assert.Equal(_tag1.Text, cloneTag1.Text);
            Assert.Equal(1, cloneTag1.Posts.Count);
            Assert.True(cloneTag1.Posts.Contains(clonePost));

            var cloneTag2 = clonePost.Tags.ElementAt(0);
            Assert.Equal(0, cloneTag2.Id);
            Assert.Equal(_tag1.Text, cloneTag2.Text);
            Assert.Equal(1, cloneTag2.Posts.Count);
            Assert.True(cloneTag2.Posts.Contains(clonePost));

            var cloneTag3 = clonePost.Tags.ElementAt(0);
            Assert.Equal(0, cloneTag3.Id);
            Assert.Equal(_tag1.Text, cloneTag3.Text);
            Assert.Equal(1, cloneTag3.Posts.Count);
            Assert.True(cloneTag3.Posts.Contains(clonePost));
        }

        [Fact]
        public async Task Post_CloneWithIncludeOfManyToManyReferenceRelationWithChildForeignKeyReleation()
        {
            // Arrange
            var entity = await TestDbContext.Set<Post>()
                .Include(b => b.Tags)
                .ThenInclude(b => b.TagHeader)
                .Where(c => c.Id == _post1.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clonePost = await TestDbContext.CloneAsync(entity);
            // Assert
            Assert.NotNull(clonePost);
            Assert.Null(clonePost.Blog);
            Assert.NotEmpty(clonePost.Tags);
            Assert.Equal(3, clonePost.Tags.Count);

            var cloneTag1 = clonePost.Tags.ElementAt(0);
            Assert.Equal(0, cloneTag1.Id);
            Assert.Equal(_tag1.Text, cloneTag1.Text);
            Assert.NotNull(cloneTag1.TagHeader);
            Assert.Equal(0, cloneTag1.TagHeader.Id);
            Assert.Equal(_tagHeader1.Header, cloneTag1.TagHeader.Header);
            Assert.Equal(1, cloneTag1.Posts.Count);
            Assert.True(cloneTag1.Posts.Contains(clonePost));

            var cloneTag2 = clonePost.Tags.ElementAt(1);
            Assert.Equal(0, cloneTag2.Id);
            Assert.Equal(_tag2.Text, cloneTag2.Text);
            Assert.NotNull(cloneTag2.TagHeader);
            Assert.Equal(0, cloneTag2.TagHeader.Id);
            Assert.Equal(_tagHeader2.Header, cloneTag2.TagHeader.Header);
            Assert.Equal(1, cloneTag2.Posts.Count);
            Assert.True(cloneTag2.Posts.Contains(clonePost));

            var cloneTag3 = clonePost.Tags.ElementAt(2);
            Assert.Equal(0, cloneTag3.Id);
            Assert.Equal(_tag3.Text, cloneTag3.Text);
            Assert.NotNull(cloneTag3.TagHeader);
            Assert.Equal(0, cloneTag3.TagHeader.Id);
            Assert.Equal(_tagHeader1.Header, cloneTag3.TagHeader.Header);
            Assert.Equal(1, cloneTag3.Posts.Count);
            Assert.True(cloneTag3.Posts.Contains(clonePost));
        }

        [Fact]
        public async Task Post_CloneWithIncludeOfManyToManyReferenceRelationWithChildOneToManyReleation()
        {
            // Arrange
            var entity = await TestDbContext.Set<Post>()
                .Include(b => b.Tags)
                .ThenInclude(b => b.TagIpAddresses)
                .Where(c => c.Id == _post1.Id)
                .AsNoTracking()
                .SingleAsync();

            // Act
            var clonePost = await TestDbContext.CloneAsync(entity);
            // Assert
            Assert.NotNull(clonePost);
            Assert.Null(clonePost.Blog);
            Assert.NotEmpty(clonePost.Tags);
            Assert.Equal(3, clonePost.Tags.Count);

            var cloneTag1 = clonePost.Tags.ElementAt(0);
            Assert.Equal(0, cloneTag1.Id);
            Assert.Equal(_tag1.Text, cloneTag1.Text);
            Assert.Null(cloneTag1.TagHeader);
            Assert.Equal(1, cloneTag1.Posts.Count);
            Assert.True(cloneTag1.Posts.Contains(clonePost));
            Assert.Equal(2, cloneTag1.TagIpAddresses.Count);

            var cloneIpAddress1 = cloneTag1.TagIpAddresses.ElementAt(0);
            Assert.Equal(0, cloneIpAddress1.Id);
            Assert.Equal(_tagIpAddress1.IpAddress, cloneIpAddress1.IpAddress);
            Assert.Equal(cloneTag1.Id, cloneIpAddress1.TagId);
            Assert.Same(cloneTag1, cloneIpAddress1.Tag);

            var cloneIpAddress2 = cloneTag1.TagIpAddresses.ElementAt(1);
            Assert.Equal(0, cloneIpAddress2.Id);
            Assert.Equal(_tagIpAddress2.IpAddress, cloneIpAddress2.IpAddress);
            Assert.Equal(cloneTag1.Id, cloneIpAddress2.TagId);
            Assert.Same(cloneTag1, cloneIpAddress2.Tag);


            var cloneTag2 = clonePost.Tags.ElementAt(1);
            Assert.Equal(0, cloneTag2.Id);
            Assert.Equal(_tag2.Text, cloneTag2.Text);
            Assert.Equal(1, cloneTag2.Posts.Count);
            Assert.True(cloneTag2.Posts.Contains(clonePost));
            Assert.Equal(1, cloneTag2.TagIpAddresses.Count);

            var cloneIpAddress3 = cloneTag2.TagIpAddresses.ElementAt(0);
            Assert.Equal(0, cloneIpAddress3.Id);
            Assert.Equal(_tagIpAddress3.IpAddress, cloneIpAddress3.IpAddress);
            Assert.Equal(cloneTag2.Id, cloneIpAddress3.TagId);
            Assert.Same(cloneTag2, cloneIpAddress3.Tag);

            var cloneTag3 = clonePost.Tags.ElementAt(2);
            Assert.Equal(0, cloneTag3.Id);
            Assert.Equal(_tag3.Text, cloneTag3.Text);
            Assert.Equal(1, cloneTag3.Posts.Count);
            Assert.True(cloneTag3.Posts.Contains(clonePost));
            Assert.Equal(0, cloneTag3.TagIpAddresses.Count);
        }

    }
}