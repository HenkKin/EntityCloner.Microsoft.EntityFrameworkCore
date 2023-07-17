namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    // ReSharper disable once UnusedTypeParameter
    public interface IIncludableClonableQueryable<out TEntity, out TProperty> : IClonableQueryable<TEntity>
    {
    }
}