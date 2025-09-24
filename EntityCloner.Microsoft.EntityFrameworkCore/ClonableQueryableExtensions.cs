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
            //params Expression<Func<object, object>>[] excludeProperties)
            where TEntity : class
        {
            return new ClonableQueryable<TEntity>(source, navigationPropertyPath, ((ClonableQueryable<TEntity>)source).Queryable.Include(navigationPropertyPath));
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            this IClonableQueryable<TEntity> source,
            Expression<Func<TEntity, TProperty>> navigationPropertyPath,
            params Expression<Func<TProperty, object>>[] excludeProperties)
            where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TProperty>(source, navigationPropertyPath.Body.ToString(), ((ClonableQueryable<TEntity>)source).Queryable.Include(navigationPropertyPath), excludeProperties);
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> IncludeSkip<TEntity, TProperty>(
             this IClonableQueryable<TEntity> source,
             Expression<Func<TEntity, TProperty>> navigationPropertyPath,
             Func<IPropertySkippableQueryable<TEntity, TProperty>, IClonableQueryable<TEntity>> skipSource)
             //params Expression<Func<TProperty, object>>[] excludeProperties)
                where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TProperty>(source, navigationPropertyPath.Body.ToString(), ((ClonableQueryable<TEntity>)source).Queryable.Include(navigationPropertyPath));
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            this IIncludableClonableQueryable<TEntity, TPreviousProperty> source,
            Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath,
            params Expression<Func<TProperty, object>>[] excludeProperties) where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TProperty>(source, navigationPropertyPath.Body.ToString(), ((IncludableClonableQueryable<TEntity, TPreviousProperty>)source).IncludableQueryable.ThenInclude(navigationPropertyPath), excludeProperties);
        }

        public static IIncludableClonableQueryable<TEntity, TPreviousProperty> SkipProperty<TEntity, TPreviousProperty, TProperty>(
        this IIncludableClonableQueryable<TEntity, TPreviousProperty> source,
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath,
        params Expression<Func<TProperty, object>>[] excludeProperties) where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TPreviousProperty>(source, navigationPropertyPath.Body.ToString(), ((IncludableClonableQueryable<TEntity, TPreviousProperty>)source).IncludableQueryable);
        }

        public static IIncludableClonableQueryable<TEntity, TPreviousProperty> SkipProperty2<TEntity, TPreviousProperty, TProperty>(
            this IIncludableClonableQueryable<TEntity, TPreviousProperty> source,
            Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath,
            params Expression<Func<TProperty, object>>[] excludeProperties) where TEntity : class
        {
            return new IncludableClonableQueryable<TEntity, TPreviousProperty>(source, navigationPropertyPath.Body.ToString(), ((IncludableClonableQueryable<TEntity, TPreviousProperty>)source).IncludableQueryable);
        }

        public static IIncludableClonableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            this IIncludableClonableQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
            Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath,
            params Expression<Func<TProperty, object>>[] excludeProperties) where TEntity : class
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
            return new IncludableClonableQueryable<TEntity, TProperty>(source, navigationPropertyPath.Body.ToString(), (IIncludableQueryable<TEntity, TProperty>)newIncludableQueryable, excludeProperties);
        }


        public static IIncludableClonableQueryable<TEntity, TProperty> ThenIncludeSkip<TEntity, TPreviousProperty, TProperty>(
          this IIncludableClonableQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
          Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath,
          Func<IPropertySkippableQueryable<TEntity, TProperty>, IClonableQueryable<TEntity>> skipSource) where TEntity : class
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

            var newIncludableQueryable = (thenIncludeMethod.MakeGenericMethod(typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)).Invoke(null, new[] { propertyValue, navigationPropertyPath }));
            return new IncludableClonableQueryable<TEntity, TProperty>(source, navigationPropertyPath.Body.ToString(), (IIncludableQueryable<TEntity, TProperty>)newIncludableQueryable);
        }
    }
}
