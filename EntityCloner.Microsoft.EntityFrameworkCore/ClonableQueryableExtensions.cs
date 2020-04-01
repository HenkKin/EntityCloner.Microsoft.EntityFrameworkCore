using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EntityCloner.Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public static class ClonableQueryableExtensions
    {
        public static IClonableQueryable<TEntity> Include<TEntity>(
            this IClonableQueryable<TEntity> source,
            [NotParameterized] string navigationPropertyPath)
            where TEntity : class
        {
            return new ClonableQueryable<TEntity>(((ClonableQueryable<TEntity>)source).Queryable.Include(navigationPropertyPath));
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            this IClonableQueryable<TEntity> source,
            Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TProperty>(((ClonableQueryable<TEntity>)source).Queryable.Include(navigationPropertyPath));
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            this IIncludableClonableQueryable<TEntity, TPreviousProperty> source,
            Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath) where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TProperty>(((IncludableClonableQueryable<TEntity, TPreviousProperty>)source).IncludableQueryable.ThenInclude(navigationPropertyPath));
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            this IIncludableClonableQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
            Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath) where TEntity : class
        {
            var propertyName = nameof(IncludableClonableQueryable<TEntity, IEnumerable<TPreviousProperty>>.IncludableQueryable);
            var property = source.GetType().GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{nameof(source)} has not an property {propertyName}");
            }
            object propertyValue = property.GetValue(source);

            var thenIncludeMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                .Where(x => x.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Single(x =>
                    x.GetParameters().Length > 0 && x.GetParameters()[0].ParameterType.IsGenericType && x.GetParameters()[0].ParameterType.GetGenericArguments().Length > 1 && x.GetParameters()[0].ParameterType.GetGenericArguments()[1].IsGenericType &&
                    x.GetParameters()[0].ParameterType.GetGenericArguments()[1].GetGenericTypeDefinition() ==
                    typeof(IEnumerable<>));

            var newIncludableQueryable = (thenIncludeMethod.MakeGenericMethod(typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)).Invoke(null, new [] { propertyValue, navigationPropertyPath }));
            return new IncludableClonableQueryable<TEntity, TProperty>((IIncludableQueryable<TEntity, TProperty>)newIncludableQueryable);
        }
    }
}
