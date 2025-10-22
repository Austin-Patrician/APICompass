using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.DeepSeek;

public class DeepSeekValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://api.deepseek.com/v1";
    public override Provider Provider => Provider.DeepSeek;

    public DeepSeekValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
        : base(httpClient, cache, semaphore)
    {
    }

    protected override async Task<ValidationResult> ValidateKeyInternalAsync(
        APIKey key,
        ValidationOptions options,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 4;
        var attempt = 0;

        while (attempt < maxRetries)
        {
            var result = await CheckDeepSeekKeyAsync(key, cancellationToken);

            if (result == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid DeepSeek key"
                };
            }

            if (result == true)
            {
                return new ValidationResult
                {
                    IsValid = true,
                    KeyInfo = key
                };
            }

            // Rate limited, retry
            key.RateLimited = true;
            attempt++;
            await Task.Delay(2000, cancellationToken);
        }

        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = "DeepSeek rate limited after retries"
        };
    }

    private async Task<bool?> CheckDeepSeekKeyAsync(APIKey key, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/user/balance");
            request.Headers.Add("Authorization", $"Bearer {key.Key}");

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return false; // Rate limited
            }

            if (!response.IsSuccessStatusCode)
            {
                return null; // Invalid
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            if (jsonResponse.TryGetProperty("balance_infos", out var balanceInfos))
            {
                foreach (var info in balanceInfos.EnumerateArray())
                {
                    if (info.TryGetProperty("total_balance", out var balance))
                    {
                        var balanceStr = balance.GetString() ?? "0";
                        key.DeepSeekBalance = $"${balanceStr} USD";
                        key.Available = double.Parse(balanceStr) > 0;
                        break;
                    }
                }
            }

            return true;
        }
        catch
        {
            return null;
        }
    }
}
