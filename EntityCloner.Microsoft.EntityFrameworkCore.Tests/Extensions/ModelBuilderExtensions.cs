using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.Extensions;

public static class ModelBuilderExtensions
{
    public static void RegisterAllEntities<Entity>(this ModelBuilder modelBuilder, params Assembly[] assemblies)
    {
        IEnumerable<Type> types = assemblies.SelectMany(a => a.GetExportedTypes()).Where(c => c.IsClass && !c.IsAbstract && c.IsPublic &&
            typeof(Entity).IsAssignableFrom(c));
        foreach (Type type in types)
            modelBuilder.Entity(type);
    }
}