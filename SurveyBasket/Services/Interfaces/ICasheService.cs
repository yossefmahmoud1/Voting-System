namespace SurveyBasket.Services.Interfaces
{
    public interface ICasheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key , CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
