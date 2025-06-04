using System.Collections.Generic;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    // ReSharper disable once UnusedTypeParameter
    public interface IClonableQueryable<out TEntity>
    {
        IDictionary<string, string> GetExcludedPropertiesRoot();
    }
}