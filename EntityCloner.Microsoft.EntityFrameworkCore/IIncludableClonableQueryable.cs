namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    // ReSharper disable once UnusedTypeParameter
    public interface IIncludableClonableQueryable<out TEntity, out TProperty> : IClonableQueryable<TEntity>
    {
    }

    public interface IPropertySkippableQueryable<out TEntity, out TProperty> : IIncludableClonableQueryable<TEntity, TProperty>
    {
    }
}