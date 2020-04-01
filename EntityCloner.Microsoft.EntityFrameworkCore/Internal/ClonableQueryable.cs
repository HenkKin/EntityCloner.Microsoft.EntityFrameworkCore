using System.Linq;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Internal
{
    internal class ClonableQueryable<TEntity> : IClonableQueryable<TEntity> where TEntity : class
    {
        public ClonableQueryable(IQueryable<TEntity> queryable)
        {
            Queryable = queryable;
        }

        public IQueryable<TEntity> Queryable { get; }
    }
}