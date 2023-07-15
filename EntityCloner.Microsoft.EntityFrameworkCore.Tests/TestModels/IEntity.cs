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
}
