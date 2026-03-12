namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public class CloneOptions
    {
        internal const bool DefaultPreservePrimaryKeyIdentity = false;
        internal const SerializationMethods DefaultSerializationMethod = SerializationMethods.NewtonsoftJson;

        /// <summary>
        /// When enabled, ensures that entities with the same original key
        /// are reused during cloning instead of duplicated.
        /// </summary>
        public bool PreservePrimaryKeyIdentity { get; set; } = DefaultPreservePrimaryKeyIdentity;

        /// <summary>
        /// Default is Newtonsoft.Json, which provides full support for complex types, polymorphic serialization/inheritance, circular references, and custom converters. System.Text.Json offers better performance but has limitations with polymorphic serialization. Choose based on your specific needs and constraints.
        /// </summary>
        public SerializationMethods SerializationMethod { get; set; } = DefaultSerializationMethod;
    }

    public enum SerializationMethods 
    {
        /// <summary>
        /// Default. Full support of serialization features, including handling of complex types, inheritance, circular references, and custom converters. But performance is worse than System.Text.Json.
        /// </summary>
        NewtonsoftJson = 0,
        /// <summary>
        /// Supports a wide range of serialization features, including handling of complex types, inheritance, circular references, and custom converters. But out of the box there is no support for Inheritance (Polymorphism). It is possible to add json attributes to your classes or create custom resolvers for the inheritance types. Performance is much better than Newtonsoft.Json.
        /// </summary>
        SystemTextJson = 1 
    }
}