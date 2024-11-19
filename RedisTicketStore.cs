using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.ServiceStackRedis;

public class RedisTicketStore(IDistributedCache cache) : ITicketStore
{
    private const string KeyPrefix = "Ticket-";

    public async Task<string> StoreAsync(AuthenticationTicket ticket) {
        var key = $"{KeyPrefix}";
        var guid= Guid.NewGuid();
        string? accountId = ticket.Principal.Claims.FirstOrDefault(c => c.Type == ClaimsName.AccountId)?.Value;
        //²»ÔÊÐíÖØ¸´µÇÂ¼
        string? isRepeatLogin = ticket.Principal.Claims.FirstOrDefault(c => c.Type == ClaimsName.IsRepeatLogin)?.Value;
        if (accountId != null && isRepeatLogin!=null)
        {
            var ticketKey = $"{KeyPrefix}{accountId}";
            bool.TryParse(isRepeatLogin, out var repeat);
            if (!repeat)
            {
               var lastCookieHashId= await cache.GetStringAsync(ticketKey);
               if (lastCookieHashId != null)
               {
                 await RemoveAsync(lastCookieHashId);
               }
            }
            key += $"{accountId}:Cookies:{guid}";
            await SetStringAsync(ticketKey, key);
        }

        await RenewAsync(key, ticket);

        return key;
    }
    public Task SetStringAsync(string ticketKey,string key)
    {
        cache.SetString(ticketKey, key);

        return Task.FromResult(0);
    }
    public Task RenewAsync(string key, AuthenticationTicket ticket) {
        var options = new DistributedCacheEntryOptions();
        var expiresUtc = ticket.Properties.ExpiresUtc;

        if (expiresUtc.HasValue) {
            options.SetAbsoluteExpiration(expiresUtc.Value);
        }
        

        ticket.Properties.Items.TryAdd("key", key);

        var val = SerializeToBytes(ticket);
        cache.Set(key, val, options);

    
        return Task.FromResult(0);
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key) {
        var bytes = cache.Get(key);
        var ticket = DeserializeFromBytes(bytes);

        return Task.FromResult(ticket ?? default);
    }

    public Task RemoveAsync(string key) {
        cache.Remove(key);

        return Task.FromResult(0);
    }

    private static byte[] SerializeToBytes(AuthenticationTicket source) => TicketSerializer.Default.Serialize(source);

    private static AuthenticationTicket? DeserializeFromBytes(byte[]? source)
      => source == null ? null : TicketSerializer.Default.Deserialize(source);
}
