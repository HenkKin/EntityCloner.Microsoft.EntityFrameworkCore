using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Internal
{
    internal class IncludableClonableQueryable<TEntity, TProperty> : ClonableQueryable<TEntity>, IIncludableClonableQueryable<TEntity, TProperty> where TEntity : class
    {
        public IncludableClonableQueryable(IClonableQueryable<TEntity> parent, string navigationPropertyPath, IIncludableQueryable<TEntity, TProperty> includableQueryable, IEnumerable<Expression<Func<TProperty, object>>> excludeProperties = null) : base(parent, navigationPropertyPath, includableQueryable)
        {
            IncludableQueryable = includableQueryable;
            ExcludeProperties = excludeProperties;
            ExcludedProperties.Select(x=>x.Value)
        }

        public IIncludableQueryable<TEntity, TProperty> IncludableQueryable { get; }
        public IEnumerable<Expression<Func<TProperty, object>>> ExcludeProperties {  get; private set; } 
    }
}