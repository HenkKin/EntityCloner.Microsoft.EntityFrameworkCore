namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public interface IIncludableClonableQueryable<out TEntity, out TProperty> : IClonableQueryable<TEntity>
    {
    }
}