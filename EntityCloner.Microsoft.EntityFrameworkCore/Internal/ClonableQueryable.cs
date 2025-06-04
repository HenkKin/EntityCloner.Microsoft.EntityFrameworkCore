using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Internal
{
    internal class ClonableQueryable<TEntity> : IClonableQueryable<TEntity> where TEntity : class
    {
        public ClonableQueryable(IQueryable<TEntity> queryable)
        {
            Queryable = queryable;
            ExcludedProperties = new Dictionary<string, string>();
        }

        public ClonableQueryable(IClonableQueryable<TEntity> parent, string navigationPropertyPath, IQueryable<TEntity> queryable, IDictionary<TEntity, object> excludedProperties = null)
        {
            NavigationPropertyPath = navigationPropertyPath;
            Queryable = queryable;
            ExcludedProperties = new Dictionary<string, string>();
        }

        public string NavigationPropertyPath { get; }
        public IQueryable<TEntity> Queryable { get; }
        public IDictionary<string, string> ExcludedProperties { get; }

        public virtual IDictionary<string, string> GetExcludedPropertiesRoot()
        {
            return ExcludedProperties;
        }
    }
}