
EntityCloner.Microsoft.EntityFrameworkCore
=========================================
[![Build Status](https://ci.appveyor.com/api/projects/status/github/HenkKin/EntityCloner.Microsoft.EntityFrameworkCore?branch=master&svg=true)](https://ci.appveyor.com/project/HenkKin/EntityCloner.Microsoft.EntityFrameworkCore) 
[![NuGet](https://img.shields.io/nuget/dt/EntityCloner.Microsoft.EntityFrameworkCore.svg)](https://www.nuget.org/packages/EntityCloner.Microsoft.EntityFrameworkCore) 
[![NuGet](https://img.shields.io/nuget/vpre/EntityCloner.Microsoft.EntityFrameworkCore.svg)](https://www.nuget.org/packages/EntityCloner.Microsoft.EntityFrameworkCore)


### Summary

Cloning entities using EntityFrameworkCore configuration.

This library is Cross-platform, supporting `net6.0`, `net7.0`, `net8.0` and `net9.0`.


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

These extension methods supports cloning an entity. Be aware: the cloned entity is not tracked by the ChangeTracker of EntityFrameworkCore. You should add it manually to the DbSet.

#### Only clone the entities you want to clone

If you provide IncludeQuery configuration, then the included entities will be cloned too. The Include and ThenInclude works same way as EntityFrameworkCore for querying with Linq. 

#### Based on EntityFrameworkCore configuration Model

It will clone the entity with related entities based on EntityFrameworkCore configuration model. 

#### Resetting properties to default value

During cloning, all PrimaryKey, ForeignKey (except for some exceptions) and ConcurrencyToken properties will be reset to default values

If a ForeignKey entity is not included, then the ForeignKey property will not be reset. This is for example in cases when you have a ForeignKey to an entity from a selection list.

#### Support for Composite PrimaryKeys

It also supports primarty keys based on multiple properties.

#### Support for IQueryable<T> and (list of) plain entities

It also supports plain classes, IQueryable<T> and all type of lists like T[], IEnumerables<T>, IList<T> ICollection<T> and the implementation variants of it.

The only requirement is, that the entity should be part of the model configuration of EntityFrameworkCore.

To use it:

```csharp
...
using EntityCloner.Microsoft.EntityFrameworkCore.EntityFrameworkCore.SqlServer;

public class YourClass
{
	   
	// This method gets called by the runtime. Use this method to add services to the container.
	public async Task YourMethod(DbContext dbContext)
	{
		var entityId = 10;

		// To clone only the entity:
		var clonedOrderEntity = await dbContext.CloneAsync<Order>(entityId);

		// To clone entity with related data
		var clonedOrderEntityWithRelatedEntities = await dbContext.CloneAsync<Order>(
			includeQuery => includeQuery
				.Include(o => o.OrderLines)
					.ThenInclude(ol => ol.Discounts)
				.Include(o => o.Customer)
					.ThenInclude(x => x.CustomerAddresses)
				.Include(o => o.Customer)
					.ThenInclude(x => x.Invoices) 
						.ThenInclude(x => x.InvoiceLines),
			entityId);

		// To clone using IQueryable
		var entityId = 10;
		var query = DbSet<TestEntity>.AsNoTracking()
				.Include(o => o.OrderLines)
					.ThenInclude(ol => ol.Discounts)
				.Include(o => o.Customer)
					.ThenInclude(x => x.CustomerAddresses)
				.Include(o => o.Customer)
					.ThenInclude(x => x.Invoices) 
						.ThenInclude(x => x.InvoiceLines)
				.Where(o => o.Id == entityId);

		var clonedOrderEntityViaQueryable = await dbContext.CloneAsync(query);

		// To clone using entity
		var entityId = 10;
		var entity = await DbSet<TestEntity>.AsNoTracking()
				.Include(o => o.OrderLines)
					.ThenInclude(ol => ol.Discounts)
				.Include(o => o.Customer)
					.ThenInclude(x => x.CustomerAddresses)
				.Include(o => o.Customer)
					.ThenInclude(x => x.Invoices) 
						.ThenInclude(x => x.InvoiceLines)
				.Where(o => o.Id == entityId)
				.SingleAsync();

		var clonedOrderEntityViaEntity = await dbContext.CloneAsync(entity);

		// To clone using list of entities
		var entityId = 10;
		var entities = await DbSet<TestEntity>.AsNoTracking()
				.Include(o => o.OrderLines)
					.ThenInclude(ol => ol.Discounts)
				.Include(o => o.Customer)
					.ThenInclude(x => x.CustomerAddresses)
				.Include(o => o.Customer)
					.ThenInclude(x => x.Invoices) 
						.ThenInclude(x => x.InvoiceLines)
				.Where(o => o.Id == entityId)
				.ToListAsync();

		var clonedOrderEntitiesViaList = await dbContext.CloneAsync(entities);
	}
}

```

### Debugging

If you want to debug the source code, thats possible. [SourceLink](https://github.com/dotnet/sourcelink) is enabled. To use it, you  have to change Visual Studio Debugging options:

Debug => Options => Debugging => General

Set the following settings:

[&nbsp;&nbsp;] Enable Just My Code

[X] Enable .NET Framework source stepping

[X] Enable source server support

[X] Enable source link support


Now you can use 'step into' (F11).
