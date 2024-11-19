// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    public class ServiceStackRedisCacheOptions : IOptions<ServiceStackRedisCacheOptions>
    {
        /// <summary>
        /// Gets or sets Redis connection string
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets maximum pool size for connections
        /// </summary>
        public int MaxPoolSize { get; set; }

        ServiceStackRedisCacheOptions IOptions<ServiceStackRedisCacheOptions>.Value => this;
    }
}