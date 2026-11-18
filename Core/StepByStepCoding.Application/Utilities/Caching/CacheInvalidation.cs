using System.Text.Json.Serialization;

namespace StepByStepCoding.Application.Utilities.Caching
{
    public abstract class CacheInvalidation
    {
        [JsonIgnore]
        public string Prefix
        {
            get;
        }

        public virtual string Key() => $"{Prefix}:";

        private CacheInvalidation()
        {
            Prefix = string.Empty;
        }

        protected CacheInvalidation(string prefix)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            Prefix = prefix;
        }
    }
}
