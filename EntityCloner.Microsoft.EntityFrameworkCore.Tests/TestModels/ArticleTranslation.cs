using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class ArticleTranslation
    {
        public Article Article { get; set; }
        public int ArticleId { get; set; }
        public string LocaleId { get; set; }
        public string Description { get; set; }
    }
}