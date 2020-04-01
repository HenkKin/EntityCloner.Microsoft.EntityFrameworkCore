using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Article
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new Collection<OrderLine>();
        public ICollection<ArticleTranslation> ArticleTranslations { get; set; } = new Collection<ArticleTranslation>();
    }
}