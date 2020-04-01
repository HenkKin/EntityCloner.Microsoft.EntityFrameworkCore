namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class OrderLine
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public decimal Quantity { get; set; }
    }
}