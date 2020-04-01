
EntityCloner.Microsoft.EntityFrameworkCore
=========================================
[![Build Status](https://ci.appveyor.com/api/projects/status/github/HenkKin/EntityCloner.Microsoft.EntityFrameworkCore?branch=master&svg=true)](https://ci.appveyor.com/project/HenkKin/EntityCloner.Microsoft.EntityFrameworkCore) 
[![BCH compliance](https://bettercodehub.com/edge/badge/HenkKin/EntityCloner.Microsoft.EntityFrameworkCore?branch=master)](https://bettercodehub.com/)
[![NuGet](https://img.shields.io/nuget/dt/EntityCloner.Microsoft.EntityFrameworkCore.svg)](https://www.nuget.org/packages/EntityCloner.Microsoft.EntityFrameworkCore) 
[![NuGet](https://img.shields.io/nuget/vpre/EntityCloner.Microsoft.EntityFrameworkCore.svg)](https://www.nuget.org/packages/EntityCloner.Microsoft.EntityFrameworkCore)


### Summary

Cloning entities using EntityFrameworkCore configuration.

This library is Cross-platform, supporting `netstandard2.1`.


### Installing EntityCloner.Microsoft.EntityFrameworkCore

You should install [EntityCloner.Microsoft.EntityFrameworkCore with NuGet](https://www.nuget.org/packages/EntityCloner.Microsoft.EntityFrameworkCore):

    Install-Package EntityCloner.Microsoft.EntityFrameworkCore

Or via the .NET Core command line interface:

    dotnet add package EntityCloner.Microsoft.EntityFrameworkCore

Either commands, from Package Manager Console or .NET Core CLI, will download and install EntityCloner.Microsoft.EntityFrameworkCore and all required dependencies.

### Dependencies

- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/)
- [Microsoft.SourceLink.GitHub](https://www.nuget.org/packages/Microsoft.SourceLink.GitHub/)

### Usage

This package provides two extension methods for `DbContext`:

- `Task<TEntity> CloneAsync<TEntity>(this DbContext source, params object[] primaryKey)`

- `Task<TEntity> CloneAsync<TEntity>(this DbContext source, Func<IClonableQueryable<TEntity>, IClonableQueryable<TEntity>> includeQuery, params object[] primaryKey)`

These extension methods supports cloning an entity.

If you provide IncludeQuery configuration, then the included entities will be cloned to. It also supports primarty keys based on multiple properties.

To use it:

```csharp
...
using EntityCloner.Microsoft.EntityFrameworkCore.EntityFrameworkCore.SqlServer;

public class YourClass
{
    ...
    
    // This method gets called by the runtime. Use this method to add services to the container.
    public async Task YourMethod(DbContext dbContext)
    {
		var entityId = 10;

		// To clone only the entity:
		var clonedOrderEntity = await dbContext.CloneAsync<Order>(entityId);
           
		// To clone entity with related data
        	var clonedOrderEntityWithRelatedEntities = await dbContext.CloneAsync<TestEntity>(includeQuery => includeQuery
                    .Include(o => o.OrderLines)
						.ThenInclude(ol => ol.Discounts)
                    .Include(o => o.Customer)
						.ThenInclude(x => x.CustomerAddresses)
                    .Include(o => o.Customer)
						.ThenInclude(x => x.Invoices) 
                            .ThenInclude(x => x.InvoiceLines) 
				entityId);
		...
		
    }
    
    ...
```

### Debugging

If you want to debug the source code, thats possible. [SourceLink](https://github.com/dotnet/sourcelink) is enabled. To use it, you  have to change Visual Studio Debugging options:

Debug => Options => Debugging => General

Set the following settings:

[&nbsp;&nbsp;] Enable Just My Code

[X] Enable source server support

[X] Enable source link support


Now you can use 'step into' (F11).
