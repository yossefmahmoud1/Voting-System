using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class CasheService : ICasheService
    {
        private readonly IDistributedCache _distributedCache;

        public CasheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (data is null)
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var json = JsonSerializer.Serialize(value);

            await _distributedCache.SetStringAsync(
                key,
                json,
                options,
                cancellationToken
            );
        }
    }
}
