using Newtonsoft.Json;

namespace EntityCloner.Microsoft.EntityFrameworkCore
{
    public class CloneOptions
    {
        internal const bool DefaultPreservePrimaryKeyIdentity = false;

        /// <summary>
        /// When enabled, ensures that entities with the same original key
        /// are reused during cloning instead of duplicated.
        /// </summary>
        public bool PreservePrimaryKeyIdentity { get; set; } = DefaultPreservePrimaryKeyIdentity;
    }
}