using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Shape //: Element
    {
        public int Id { get; set; }
        public string Title { get; set; }
        //public Element Element { get; set; }
        public IList<Shape> Predecessors { get; set; } = new List<Shape>();
    }
}