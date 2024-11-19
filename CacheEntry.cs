using System;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    internal struct CacheEntry
    {

        public string Value { get; set; } 
        public TimeSpan SlidingExpiration { get; set; }
    }
}
