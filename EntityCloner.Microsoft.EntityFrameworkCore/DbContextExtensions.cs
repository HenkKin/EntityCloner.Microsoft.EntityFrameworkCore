using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static async Task<IList<TEntity>> CloneAsync<TEntity>(this DbContext source, IQueryable<TEntity> queryable)
            where TEntity : class
        {
            var entities = await queryable.AsNoTracking().ToListAsync();

            var clonedEntities = source.InternalCloneCollection(new Dictionary<object, object>(), typeof(TEntity), entities);

            return clonedEntities.Cast<TEntity>().ToList();
        }

        public static async Task<TEntity> CloneAsync<TEntity>(this DbContext source, TEntity entityOrListOfEntities)
            where TEntity : class
        {
            if((typeof(TEntity).IsGenericType && typeof(TEntity).GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                (typeof(TEntity).GetInterfaces().Any(i=>i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
            {
                var entityType = (typeof(TEntity).HasElementType ? typeof(TEntity).GetElementType() : typeof(TEntity).GenericTypeArguments[0]) ?? typeof(TEntity);

                if (source.Model.FindEntityType(entityType) == null)
                {
                    throw new ArgumentException($"Argument should be a known entity of the DbContext", nameof(entityOrListOfEntities));
                }

                var clonedEntities = source.InternalCloneCollection(new Dictionary<object, object>(), entityType, (IEnumerable)entityOrListOfEntities);

                return (TEntity)clonedEntities;
            }

            if (source.Model.FindEntityType(typeof(TEntity)) == null)
            {
                throw new ArgumentException($"Argument should be a known entity of the DbContext", nameof(entityOrListOfEntities));
            }

            var clonedEntity = (TEntity)source.InternalClone(entityOrListOfEntities, new Dictionary<object, object>());

            return await Task.FromResult(clonedEntity);
        }

        public static Task<TEntity> CloneAsync<TEntity>(this DbContext source, params object[] primaryKey)
            where TEntity : class
        {
            return source.CloneAsync<TEntity>(includeQuery => includeQuery, primaryKey);
        }

        public static async Task<TEntity> CloneAsync<TEntity>(this DbContext source, Func<IClonableQueryable<TEntity>, IClonableQueryable<TEntity>> includeQuery, params object[] primaryKey)
            where TEntity : class
        {
            var  primaryKeyProperties = source.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties?.Select(p=>p.PropertyInfo).ToList();
            if (primaryKeyProperties == null)
            {
                throw new NotSupportedException("CloneAsync only can handle types with PrimaryKey configuration'");
            }

            var primaryKeyExpression = CreatePrimaryKeyExpression<TEntity>(primaryKey, primaryKeyProperties);

            var query = source.Set<TEntity>().AsNoTracking().Where(primaryKeyExpression);

            var clonableQueryable = new ClonableQueryable<TEntity>(query);

            clonableQueryable = (ClonableQueryable<TEntity>)includeQuery(clonableQueryable);

            var entity = await clonableQueryable.Queryable.SingleAsync();

            var clonedEntity = (TEntity)source.InternalClone(entity, new Dictionary<object, object>());

            return clonedEntity;
        }

        private static object ConvertToCollectionType(Type collectionType, Type entityType, IList collectionValue)
        {
            if (collectionType.IsInterface)
            {
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(entityType), collectionValue);
                return list;
            }

            if (collectionType.IsArray)
            {
                var array = Array.CreateInstance(entityType, collectionValue.Count);
                for (int i = 0; i < collectionValue.Count; i++)
                {
                    array.SetValue(collectionValue[i], i);
                }

                return array;
            }

            return Activator.CreateInstance(collectionType, collectionValue);
        }

        private static Expression<Func<TEntity, bool>> CreatePrimaryKeyExpression<TEntity>(object[] primaryKey, List<PropertyInfo> primaryKeyProperties)
            where TEntity : class
        {
            Expression<Func<TEntity, bool>> primaryKeyExpression = e => true;

            for (int i = 0; i < primaryKeyProperties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var idPart = primaryKey[i];

                if (idPart?.GetType() != primaryKeyProperty.PropertyType)
                {
                    throw new NotSupportedException(
                        $"CloneAsync only can handle id of type '{primaryKeyProperty.PropertyType.FullName}', passed id of type '{idPart?.GetType().FullName}'");
                }

                var idPartPredicate = BuildPredicate<TEntity>(primaryKeyProperty.Name, ExpressionType.Equal, idPart);
                primaryKeyExpression = AndAlso(primaryKeyExpression, idPartPredicate);
            }

            return primaryKeyExpression;
        }

        private static Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, ExpressionType comparison, object value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var left = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);
            var body = MakeBinary(comparison, left, value);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    expr1.Body,
                    Expression.Invoke(expr2, param)), param);
        }

        private static Expression MakeBinary(ExpressionType type, Expression left, object value)
        {
            var right = Expression.Constant(value, left.Type);
            return Expression.MakeBinary(type, left, right);
        }

        private static object InternalClone(this DbContext source, object entity, Dictionary<object, object> references)
        {
            if (references.ContainsKey(entity))
            {
                return references[entity];
            }

            var clonedEntity = source.Entry(entity).CurrentValues.ToObject();

            references.Add(entity, clonedEntity);

            source.ResetEntityProperties(entity, clonedEntity);

            source.ResetNavigationProperties(entity, references, clonedEntity);

            return clonedEntity;
        }

        private static void ResetNavigationProperties(this DbContext source, object entity, Dictionary<object, object> references, object clonedEntity)
        {
            foreach (var navigation in source.Model.FindEntityType(entity.GetType()).GetNavigations())
            {
                var navigationValue = navigation.PropertyInfo.GetValue(entity);

                if (navigation.IsDependentToPrincipal() && navigationValue != null)
                {
                    foreach (var foreignKeyProperty in navigation.ForeignKey.Properties)
                    {
                        ResetProperty(foreignKeyProperty, clonedEntity);
                    }
                }

                if (navigationValue != null)
                {
                    if (navigation.IsCollection())
                    {
                        var collection = source.InternalCloneCollection(references, navigation.ClrType.GenericTypeArguments[0], (IEnumerable)navigationValue);
                        navigation.PropertyInfo.SetValue(clonedEntity, collection);
                    }
                    else
                    {
                        var clonedPropertyValue = source.InternalClone(navigationValue, references);
                        navigation.PropertyInfo.SetValue(clonedEntity, clonedPropertyValue);
                    }
                }
            }
        }

        private static IList InternalCloneCollection(this DbContext source, Dictionary<object, object> references, Type collectionItemType, IEnumerable collectionValue)
        {
            var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionItemType));
            foreach (var item in collectionValue)
            {
                var clonedItemValue = source.InternalClone(item, references);
                list.Add(clonedItemValue);
            }

            return (IList)ConvertToCollectionType(collectionValue.GetType(), collectionItemType, list);
        }

        private static void ResetEntityProperties(this DbContext source, object entity, object clonedEntity)
        {
            foreach (var property in source.Model.FindEntityType(entity.GetType()).GetProperties())
            {
                if (property.IsConcurrencyToken)
                {
                    ResetProperty(property, clonedEntity);
                }

                if (property.IsPrimaryKey())
                {
                    ResetProperty(property, clonedEntity);
                }
            }
        }

        private static void ResetProperty(IProperty property, object entity)
        {
            if (property.PropertyInfo == null)
            {
                return;
            }

            property.PropertyInfo.SetValue(entity, property.IsNullable ? null : GetDefault(property.ClrType));
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
