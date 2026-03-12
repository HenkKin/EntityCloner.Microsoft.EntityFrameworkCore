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
        #region Cloning
        public static async Task<IList<TEntity>> CloneAsync<TEntity>(this DbContext source, IQueryable<TEntity> queryable, CloneOptions options = default)
            where TEntity : class
        {
            var entities = await queryable.AsNoTracking().ToListAsync();

            var clonedEntities = source.InternalCloneCollection(new Dictionary<object, object>(), typeof(TEntity), null, null, entities, options ?? new CloneOptions());

            return clonedEntities.Cast<TEntity>().ToList();
        }

        public static async Task<TEntity> CloneAsync<TEntity>(this DbContext source, TEntity entityOrListOfEntities, CloneOptions options = default)
            where TEntity : class
        {
            IReadOnlyEntityType entityType;
            if ((typeof(TEntity).IsGenericType && typeof(TEntity).GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                (typeof(TEntity).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
            {
                var entityClrType = (typeof(TEntity).HasElementType ? typeof(TEntity).GetElementType() : typeof(TEntity).GenericTypeArguments[0]) ?? typeof(TEntity);

                entityType = source.FindCurrentEntityType(entityClrType, null, null);
                if (entityType == null)
                {
                    throw new ArgumentException("Argument should be a known entity of the DbContext", nameof(entityOrListOfEntities));
                }

                var clonedEntities = source.InternalCloneCollection(new Dictionary<object, object>(), entityClrType, null, null, (IEnumerable)entityOrListOfEntities, options ?? new CloneOptions());

                return (TEntity)clonedEntities;
            }

            entityType = source.FindCurrentEntityType(typeof(TEntity), null, null);

            if (entityType == null)
            {
                throw new ArgumentException("Argument should be a known entity of the DbContext", nameof(entityOrListOfEntities));
            }

            var clonedEntity = (TEntity)source.InternalClone(entityOrListOfEntities, null, null, new Dictionary<object, object>(), options ?? new CloneOptions());

            return await Task.FromResult(clonedEntity);
        }

        public static Task<TEntity> CloneAsync<TEntity>(this DbContext source, params object[] primaryKey)
            where TEntity : class
        {
            return CloneAsync<TEntity>(source, new CloneOptions(), primaryKey);
        }

        public static Task<TEntity> CloneAsync<TEntity>(this DbContext source, CloneOptions options, params object[] primaryKey)
            where TEntity : class
        {
            return CloneAsync<TEntity>(source, includeQuery => includeQuery, options ?? new CloneOptions(), primaryKey);
        }

        public static Task<TEntity>  CloneAsync<TEntity>(this DbContext source, Func<IClonableQueryable<TEntity>, IClonableQueryable<TEntity>> includeQuery, params object[] primaryKey)
            where TEntity : class
        {
            return CloneAsync(source, includeQuery, new CloneOptions(), primaryKey);
        }

        public static async Task<TEntity> CloneAsync<TEntity>(this DbContext source, Func<IClonableQueryable<TEntity>, IClonableQueryable<TEntity>> includeQuery, CloneOptions options, params object[] primaryKey)
            where TEntity : class
        {
            var entityType = source.FindCurrentEntityType(typeof(TEntity), null, null);
            var primaryKeyProperties = entityType?.FindPrimaryKey()?.Properties.Select(p => p.PropertyInfo).ToList();
            if (primaryKeyProperties == null)
            {
                throw new NotSupportedException("CloneAsync only can handle types with PrimaryKey configuration'");
            }

            var primaryKeyExpression = CreatePrimaryKeyExpression<TEntity>(primaryKey, primaryKeyProperties);

            var query = source.Set<TEntity>().AsNoTracking().Where(primaryKeyExpression);

            var clonableQueryable = new ClonableQueryable<TEntity>(query);

            clonableQueryable = (ClonableQueryable<TEntity>)includeQuery(clonableQueryable);

            var entity = await clonableQueryable.Queryable.SingleAsync();

            var clonedEntity = (TEntity)source.InternalClone(entity, null, null, new Dictionary<object, object>(), new CloneOptions());

            return clonedEntity;
        }

        private static object InternalClone(this DbContext source, object entity, string definingNavigationName, IReadOnlyEntityType definingEntityType, Dictionary<object, object> references, CloneOptions options)
        {
            if (!options.PreservePrimaryKeyIdentity)
            {
                if (references.ContainsKey(entity))
                {
                    return references[entity];
                }
            }

            object clonedEntity;

            if(options.SerializationMethod == SerializationMethods.SystemTextJson)
            {
                System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new()
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                    WriteIndented = true
                };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(entity, jsonSerializerOptions);
                clonedEntity = System.Text.Json.JsonSerializer.Deserialize(jsonString, entity.GetType(), jsonSerializerOptions);
            }
            else // Default SerializationMethods.NewtonsoftJson
            {
                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All,
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(entity, jsonSettings);
                clonedEntity = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, entity.GetType(), jsonSettings);
            }

            var primaryKeyStringOrInstance = options.PreservePrimaryKeyIdentity ? source.CreatePrimaryKeyStringOrInstance(entity, options) : entity;
            var isEarlierClonedEntity = references.ContainsKey(primaryKeyStringOrInstance);
            if (options.PreservePrimaryKeyIdentity && isEarlierClonedEntity)
            {
                var earlierClonedEntity = references[primaryKeyStringOrInstance];
                source.MergeNavigationProperties(earlierClonedEntity, clonedEntity, definingNavigationName, definingEntityType);
                return earlierClonedEntity;
            }
            else
            {
                references.Add(primaryKeyStringOrInstance, clonedEntity);
            }

            // source.CloneOwnedEntityProperties(entity, definingNavigationName, definingEntityType, references, clonedEntity);

            source.ResetEntityProperties(entity, definingNavigationName, definingEntityType, clonedEntity);

            source.ResetNavigationProperties(entity, definingNavigationName, definingEntityType, references, clonedEntity, options);

            return clonedEntity;
        }

        //private static void CloneOwnedEntityProperties(this DbContext source, object entity, string definingNavigationName, IEntityType definingEntityType, Dictionary<object, object> references, object clonedEntity)
        //{
        //    foreach (var navigation in source.FindCurrentEntityType(entity.GetType(), definingNavigationName, definingEntityType).GetNavigations())
        //    {
        //        var navigationValue = navigation.PropertyInfo.GetValue(entity);

        //        if(navigation.ForeignKey.DeclaringEntityType.DefiningEntityType != null && navigation.ForeignKey.DeclaringEntityType.DefiningNavigationName != null)
        //        {
        //            navigation.PropertyInfo.SetValue(clonedEntity, navigationValue);
        //        }            
        //    }
        //}

        private static IEnumerable InternalCloneCollection(this DbContext source, Dictionary<object, object> references, Type collectionItemType, string definingNavigationName, IReadOnlyEntityType definingEntityType, IEnumerable collectionValue, CloneOptions options)
        {
            // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations#collection-types
            // The underlying collection instance must be implement ICollection<T>, and must have a working Add method. It is common to use List<T> or HashSet<T>
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionItemType));
            if (list == null)
            {
                throw new ArgumentNullException(nameof(collectionItemType));
            }
            foreach (var item in collectionValue)
            {
                var clonedItemValue = source.InternalClone(item, definingNavigationName, definingEntityType, references, options);
                list.Add(clonedItemValue);
            }

            return (IEnumerable)ConvertToCollectionType(collectionValue.GetType(), collectionItemType, list);
        }

        #endregion

        #region Helpers
        private static IReadOnlyEntityType FindCurrentEntityType(this DbContext source, Type entityClrType, string definingNavigationName, IReadOnlyEntityType definingEntityType)
        {
            if (!string.IsNullOrEmpty(definingNavigationName) && definingEntityType != null)
            {
                var entity = source.Model.FindEntityType(entityClrType, definingNavigationName, definingEntityType);
                if (entity != null)
                {
                    return entity;
                }
            }
            return source.Model.FindEntityType(entityClrType);
        }

        private static object CreatePrimaryKeyStringOrInstance(this DbContext source, object entity, CloneOptions options)
        {
            var entityType = source.FindCurrentEntityType(entity.GetType(), null, null);
            var primaryKeyProperties = entityType?.FindPrimaryKey()?.Properties.Select(p => p.PropertyInfo).ToList();
            if (primaryKeyProperties == null)
            {
                // no primary key, then use instance as key.
                return entity;
            }

            string primaryKeyString = null;
            Func<object, string> serializeMethod;
            
            if (options.SerializationMethod == SerializationMethods.SystemTextJson)
            {
                System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new()
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                    WriteIndented = true
                };
                serializeMethod = (object idPart) => System.Text.Json.JsonSerializer.Serialize(idPart, jsonSerializerOptions);
            }
            else // Default SerializationMethods.NewtonsoftJson
            {
                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All,
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                };
                serializeMethod = (object idPart) => Newtonsoft.Json.JsonConvert.SerializeObject(idPart, jsonSettings);
            }

            for (int i = 0; i < primaryKeyProperties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var idPart = primaryKeyProperty.GetValue(entity);

                if (idPart?.GetType() != primaryKeyProperty.PropertyType)
                {
                    throw new NotSupportedException(
                        $"CreatePrimaryKeyString only can handle id of type '{primaryKeyProperty.PropertyType.FullName}', passed id of type '{idPart?.GetType().FullName}'");
                }

                string separator = "";
                if (!string.IsNullOrWhiteSpace(primaryKeyString))
                {
                    separator = "-";
                }

                string idPartString = idPart.GetType() == typeof(string) ? idPart?.ToString() : serializeMethod(idPart);
                primaryKeyString = primaryKeyString + separator + idPartString;
            }

            return entityType.Name + "-" + primaryKeyString;
        }

        private static object ConvertToCollectionType(Type collectionType, Type entityType, ICollection collectionValue)
        {
            if (collectionType.IsInterface)
            {
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(entityType), collectionValue);
                return list;
            }

            if (collectionType.IsArray)
            {
                var array = Array.CreateInstance(entityType, collectionValue.Count);
                var index = 0;
                foreach (var item in collectionValue)
                {
                    array.SetValue(item, index);
                    index++;
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
                primaryKeyExpression = primaryKeyExpression.AndAlso(idPartPredicate);
            }

            return primaryKeyExpression;
        }

        private static Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, ExpressionType comparison, object value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var left = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);

            // Create a strongly-typed closure to  force EF Core to create a parameter
            var wrapperType = typeof(Wrapper<>).MakeGenericType(left.Type);
            var wrapper = Activator.CreateInstance(wrapperType, value);

            var wrapperExpression = Expression.Constant(wrapper);
            var valueExpression = Expression.Property(wrapperExpression, "PkValue");

            var body = Expression.MakeBinary(comparison, left, valueExpression);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private class Wrapper<TValue>
        {
            public TValue PkValue { get; }
            public Wrapper(TValue pkValue) => PkValue = pkValue;
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];

            // Rebind expr2's parameter naar expr1's parameter
            var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
            var body2 = visitor.Visit(expr2.Body);

            var body = Expression.AndAlso(expr1.Body, body2);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _oldParam ? _newParam : base.VisitParameter(node);
        }

        #endregion

        #region Reset Properties

        private static void ResetNavigationProperties(this DbContext source, object entity, string definingNavigationName, IReadOnlyEntityType definingEntityType, Dictionary<object, object> references, object clonedEntity, CloneOptions options)
        {
            var entityType = source.FindCurrentEntityType(entity.GetType(), definingNavigationName, definingEntityType);
            if (entityType != null)
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    ResetNavigationProperty(source, entity, references, clonedEntity, navigation, options);
                }

                foreach (var navigation in entityType.GetSkipNavigations())
                {
                    ResetSkipNavigationProperty(source, entity, references, clonedEntity, navigation, options);
                }
            }
        }

        private static void ResetNavigationProperty<TNavigation>(DbContext source, object entity, Dictionary<object, object> references, object clonedEntity, TNavigation navigation, CloneOptions options)
            where TNavigation : IReadOnlyNavigation
        {
            var navigationValue = navigation.PropertyInfo?.GetValue(entity);
            if (navigationValue != null)
            {
                if (navigation.IsOnDependent)
                {
                    foreach (var foreignKeyProperty in navigation.ForeignKey.Properties)
                    {
                        ResetProperty(foreignKeyProperty, clonedEntity);
                    }
                }
           
                if (navigation.IsCollection)
                {
                    var collection = source.InternalCloneCollection(references, navigation.ClrType.GenericTypeArguments[0], navigation.Name, navigation.DeclaringEntityType, (IEnumerable)navigationValue, options);
                    navigation.PropertyInfo.SetValue(clonedEntity, collection);
                }
                else
                {
                    var clonedPropertyValue = source.InternalClone(navigationValue, navigation.Name, navigation.DeclaringEntityType, references, options);
                    navigation.PropertyInfo.SetValue(clonedEntity, clonedPropertyValue);
                }
            }
        }

        private static void ResetSkipNavigationProperty<TNavigation>(DbContext source, object entity, Dictionary<object, object> references, object clonedEntity, TNavigation navigation, CloneOptions options)
            where TNavigation : IReadOnlySkipNavigation
        {
            var navigationValue = navigation.PropertyInfo?.GetValue(entity);
            if (navigationValue != null)
            {
                if (navigation.ForeignKey != null && navigation.IsOnDependent)
                {
                    foreach (var foreignKeyProperty in navigation.ForeignKey.Properties)
                    {
                        ResetProperty(foreignKeyProperty, clonedEntity);
                    }
                }

                if (navigation.IsCollection)
                {
                    var collection = source.InternalCloneCollection(references, navigation.ClrType.GenericTypeArguments[0], navigation.Name, navigation.DeclaringEntityType, (IEnumerable)navigationValue, options);
                    navigation.PropertyInfo.SetValue(clonedEntity, collection);
                }
                else
                {
                    var clonedPropertyValue = source.InternalClone(navigationValue, navigation.Name, navigation.DeclaringEntityType, references, options);
                    navigation.PropertyInfo.SetValue(clonedEntity, clonedPropertyValue);
                }
            }
        }

        private static void ResetEntityProperties(this DbContext source, object entity, string definingNavigationName, IReadOnlyEntityType definingEntityType, object clonedEntity)
        {
            foreach (var property in source.FindCurrentEntityType(entity.GetType(), definingNavigationName, definingEntityType).GetProperties())
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

        private static void ResetProperty(IReadOnlyProperty property, object entity)
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

        #endregion

        #region Merge properties

        private static void MergeNavigationProperties(this DbContext source, object toEntity, object fromEntity, string definingNavigationName, IReadOnlyEntityType definingEntityType)
        {
            var entityType = source.FindCurrentEntityType(toEntity.GetType(), definingNavigationName, definingEntityType);
            if (entityType != null)
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    MergeNavigationProperty(source, toEntity, fromEntity, navigation);
                }

                foreach (var navigation in entityType.GetSkipNavigations())
                {
                    MergeSkipNavigationProperty(source, toEntity, fromEntity, navigation);
                }
            }
        }

        private static void MergeNavigationProperty<TNavigation>(DbContext source, object toEntity, object fromEntity, TNavigation navigation)
                where TNavigation : IReadOnlyNavigation
        {
            var toEntityNavigationValue = navigation.PropertyInfo?.GetValue(toEntity);
            if (toEntityNavigationValue == null)
            {
                //if (navigation.IsOnDependent)
                //{
                //    foreach (var foreignKeyProperty in navigation.ForeignKey.Properties)
                //    {
                //        MergeProperty(foreignKeyProperty, toEntity, fromEntity);
                //    }
                //}

                var fromEntityNavigationValue = navigation.PropertyInfo?.GetValue(fromEntity);
                navigation.PropertyInfo.SetValue(toEntity, fromEntityNavigationValue);
            }
        }

        private static void MergeSkipNavigationProperty<TNavigation>(DbContext source, object toEntity, object fromEntity, TNavigation navigation)
            where TNavigation : IReadOnlySkipNavigation
        {
            var toEntityNavigationValue = navigation.PropertyInfo?.GetValue(toEntity);
            if (toEntityNavigationValue == null)
            {
                //if (navigation.ForeignKey != null && navigation.IsOnDependent)
                //{
                //    foreach (var foreignKeyProperty in navigation.ForeignKey.Properties)
                //    {
                //        MergeProperty(foreignKeyProperty, toEntity, fromEntity);
                //    }
                //}
            
                var fromEntityNavigationValue = navigation.PropertyInfo?.GetValue(fromEntity);
                navigation.PropertyInfo.SetValue(toEntity, fromEntityNavigationValue);
            }
        }

        //private static void MergeProperty(IReadOnlyProperty property, object entity, object clonedEntity)
        //{
        //    if (property.PropertyInfo == null)
        //    {
        //        return;
        //    }

        //    var firstValue = property.PropertyInfo.GetValue(entity);
        //    var secondValue = property.PropertyInfo.GetValue(clonedEntity);
        //    if (firstValue == null)
        //    {
        //        property.PropertyInfo.SetValue(entity, secondValue);
        //    }
        //}

        #endregion
    }
}