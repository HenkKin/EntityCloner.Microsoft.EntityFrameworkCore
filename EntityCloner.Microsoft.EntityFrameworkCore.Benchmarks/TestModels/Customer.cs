using System;
using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Customer
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; }
        public DateTime BirthDate { get; set; }
        public Address Address { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}