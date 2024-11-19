// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    public static class DistributedStoreDataProtectionBuilderExtensions
    {
        public static IDataProtectionBuilder PersistKeysToDistributedStore(this IDataProtectionBuilder builder, IRedisClientsManager redisClientsManager)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
      
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new RedisXmlRepository(redisClientsManager);
            });
            return builder;
        }
    }
}