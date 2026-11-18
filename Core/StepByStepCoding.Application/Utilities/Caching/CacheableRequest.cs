using StepByStepCoding.Application.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StepByStepCoding.Application.Utilities.Caching
{
    public abstract class CacheableRequest
    {
        [JsonIgnore]
        public string Prefix
        {
            get;
        }

        private CacheableRequest()
        {
            Prefix = string.Empty;
        }

        protected CacheableRequest(string prefix)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            Prefix = prefix;
        }

        public virtual string Key()
            => $"{Prefix}:{GetType().FullName}:{JsonSerializer.Serialize(this, GetType(), SerializerOptions)}";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    public abstract class CacheableRequest<TResponse>(string prefix)
        : CacheableRequest(prefix), IRequest<TResponse>
    {
    }
}
