using System;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class SkipResetAttribute : Attribute
    {
    }
}