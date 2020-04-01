using Microsoft.EntityFrameworkCore.Query;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Internal
{
    internal class IncludableClonableQueryable<TEntity, TProperty> : ClonableQueryable<TEntity>, IIncludableClonableQueryable<TEntity, TProperty> where TEntity : class
    {
        public IncludableClonableQueryable(IIncludableQueryable<TEntity, TProperty> includableQueryable) : base(includableQueryable)
        {
            IncludableQueryable = includableQueryable;
        }

        public IIncludableQueryable<TEntity, TProperty> IncludableQueryable { get; }
    }
}