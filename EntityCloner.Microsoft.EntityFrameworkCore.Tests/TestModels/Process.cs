using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Process// : Element
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public IList<Shape> Shapes { get; set; } = new List<Shape>();
    }
}