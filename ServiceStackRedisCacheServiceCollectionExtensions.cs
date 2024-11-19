using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

namespace Microsoft.Extensions.Caching.ServiceStackRedis;

public static class RedisCacheServiceCollectionExtensions
{
    /// <summary>
    /// Configures ServiceStack Redis distributed cache and integrates it with Data Protection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="section">The configuration section containing Redis settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddDistributedServiceStackRedisCache(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        // Argument validation
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (section == null)
            throw new ArgumentNullException(nameof(section));

        // Bind options from configuration
        services.AddOptions();
        services.Configure<ServiceStackRedisCacheOptions>(section);

        // Retrieve configured options
        var options = section.Get<ServiceStackRedisCacheOptions>();
        if (options == null || string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("Invalid Redis configuration. Connection string is required.", nameof(section));

        // Create Redis Manager Pool
        var redisManagerPool = new RedisManagerPool(
            options.ConnectionString,
            new RedisPoolConfig { MaxPoolSize = options.MaxPoolSize });

        // Register Redis-related services
        services.AddSingleton<IRedisClientsManager>(redisManagerPool);

        // Configure data protection to use Redis
        services
            .AddDataProtection(dataProtectionOptions =>
            {
                dataProtectionOptions.ApplicationDiscriminator = "To-baba"; // Application-specific key isolation
            })
            .SetApplicationName("To-baba") // Application name for shared key storage
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC, // AES with 256-bit keys
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256    // HMAC-SHA256 for integrity validation
            })
            .PersistKeysToDistributedStore(redisManagerPool);

        // Register distributed cache implementations
        services.AddSingleton<IDistributedCache, ServiceStackRedisCache>();
        services.AddSingleton<RedisCache>();

        return services;
    }
}