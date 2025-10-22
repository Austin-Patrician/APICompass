using System.Diagnostics;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Validators.Base;

public abstract class BaseKeyValidator : IKeyValidator
{
    protected readonly HttpClient HttpClient;
    protected readonly IValidationCache Cache;
    protected readonly SemaphoreSlim Semaphore;

    public abstract Provider Provider { get; }

    protected BaseKeyValidator(
        HttpClient httpClient,
        IValidationCache cache,
        SemaphoreSlim semaphore)
    {
        HttpClient = httpClient;
        Cache = cache;
        Semaphore = semaphore;
    }

    public async Task<ValidationResult> ValidateAsync(
        APIKey key,
        ValidationOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check cache first
            if (options.UseCache)
            {
                var cacheKey = GetCacheKey(key.Key);
                var cached = await Cache.GetAsync<ValidationResult>(cacheKey, cancellationToken);
                if (cached != null)
                {
                    return cached;
                }
            }

            // Acquire semaphore
            await Semaphore.WaitAsync(cancellationToken);
            try
            {
                // Perform validation
                var result = await ValidateKeyInternalAsync(key, options, cancellationToken);
                result.ValidationDuration = stopwatch.Elapsed;

                // Cache successful results
                if (options.UseCache && result.IsValid)
                {
                    var cacheKey = GetCacheKey(key.Key);
                    await Cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
                }

                return result;
            }
            finally
            {
                Semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message,
                ValidationDuration = stopwatch.Elapsed
            };
        }
    }

    protected abstract Task<ValidationResult> ValidateKeyInternalAsync(
        APIKey key,
        ValidationOptions options,
        CancellationToken cancellationToken);

    protected virtual string GetCacheKey(string apiKey)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(apiKey));
        var hashString = Convert.ToHexString(hash);
        return $"key:validation:{Provider}:{hashString}";
    }
}
