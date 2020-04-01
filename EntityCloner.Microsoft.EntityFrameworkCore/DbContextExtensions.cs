using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityCloner.Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static Task<TEntity> CloneAsync<TEntity>(this DbContext source, params object[] primaryKey)
            where TEntity : class
        {
            return CloneAsync<TEntity>(source, includeQuery => includeQuery, primaryKey);
        }

        public static async Task<TEntity> CloneAsync<TEntity>(this DbContext source, Func<IClonableQueryable<TEntity>, IClonableQueryable<TEntity>> includeQuery, params object[] primaryKey)
            where TEntity : class
        {
            var  primaryKeyProperties = source.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties?.Select(p=>p.PropertyInfo).ToList();
            if (primaryKeyProperties == null)
            {
                throw new NotSupportedException($"CloneAsync only can handle types with PrimaryKey configuration'");
            }

            Expression<Func<TEntity, bool>> primaryKeyExpression = e => true;

            for (int i = 0; i < primaryKeyProperties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var idPart = primaryKey[i];

                if (idPart?.GetType() != primaryKeyProperty.PropertyType)
                {
                    throw new NotSupportedException($"CloneAsync only can handle id of type '{primaryKeyProperty.PropertyType.FullName}', passed id of type '{idPart?.GetType().FullName}'");
                }

                var idPartPredicate = BuildPredicate<TEntity>(primaryKeyProperty.Name, ExpressionType.Equal, idPart.ToString());
                primaryKeyExpression = AndAlso(primaryKeyExpression, idPartPredicate);
            }

            var query = source.Set<TEntity>().AsNoTracking().Where(primaryKeyExpression);

            IClonableQueryable<TEntity> cloneableQueryable = new ClonableQueryable<TEntity>(query);

            cloneableQueryable = includeQuery(cloneableQueryable);

            var sqlServerClonableQueryable = (ClonableQueryable<TEntity>)cloneableQueryable;

            var entity = await sqlServerClonableQueryable.Queryable.SingleAsync();

            var clonedEntity = (TEntity)source.InternalClone(entity, new Dictionary<object, object>());
            return clonedEntity;
        }

        private static Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, ExpressionType comparison, string value)
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

        private static Expression MakeBinary(ExpressionType type, Expression left, string value)
        {
            object typedValue = value;
            if (left.Type != typeof(string))
            {
                if (string.IsNullOrEmpty(value))
                {
                    typedValue = null;
                    if (Nullable.GetUnderlyingType(left.Type) == null)
                        left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
                }
                else
                {
                    var valueType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
                    typedValue = valueType.IsEnum ? Enum.Parse(valueType, value) :
                        valueType == typeof(Guid) ? Guid.Parse(value) :
                        Convert.ChangeType(value, valueType);
                }
            }
            var right = Expression.Constant(typedValue, left.Type);
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

            foreach (var property in source.Model.FindEntityType(entity.GetType()).GetProperties())
            {
                if (property.IsConcurrencyToken)
                {
                    ResetProperty(property, clonedEntity);
                }

                if (property.IsPrimaryKey())
                {
                    // TODO: bij Translations hebben we een samengestelde primary key, LocaleId gaat dan ook leeg
                    ResetProperty(property, clonedEntity);
                }
            }

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
                        var list = (IList)Activator.CreateInstance(typeof(Collection<>).MakeGenericType(navigation.ClrType.GenericTypeArguments[0]));
                        foreach (var item in (IEnumerable)navigationValue)
                        {
                            var clonedItemValue = source.InternalClone(item, references);
                            list.Add(clonedItemValue);
                        }

                        navigation.PropertyInfo.SetValue(clonedEntity, list);
                    }
                    else
                    {
                        var clonedPropertyValue = source.InternalClone(navigationValue, references);
                        navigation.PropertyInfo.SetValue(clonedEntity, clonedPropertyValue);
                    }
                }
            }

            return clonedEntity;
        }

        private static void ResetProperty(IProperty property, object entity)
        {
            if (property.PropertyInfo == null)
            {
                return;
            }

            if (property.IsNullable)
            {
                property.PropertyInfo.SetValue(entity, null);
            }
            else
            {
                property.PropertyInfo.SetValue(entity, GetDefault(property.ClrType));
            }
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
