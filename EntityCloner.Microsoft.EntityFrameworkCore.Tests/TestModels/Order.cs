using System;
using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public byte[] RowVersion { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Address InstallationAddress { get; set; }
        public bool IsDeleted { get; set; }
        public int TenantId { get; set; }
        public string Description { get; set; }
        public Money TotalOrderPrice { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
