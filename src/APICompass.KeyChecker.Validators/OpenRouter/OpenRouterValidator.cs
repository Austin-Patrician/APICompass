using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.OpenRouter;

public class OpenRouterValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://openrouter.ai/api/v1";
    public override Provider Provider => Provider.OpenRouter;

    public OpenRouterValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
        : base(httpClient, cache, semaphore)
    {
    }

    protected override async Task<ValidationResult> ValidateKeyInternalAsync(
        APIKey key,
        ValidationOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/auth/key");
            request.Headers.Add("Authorization", $"Bearer {key.Key}");

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid OpenRouter key"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            if (jsonResponse.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("usage", out var usage))
                {
                    key.Usage = usage.GetDecimal();
                }
                
                if (data.TryGetProperty("limit", out var limit))
                {
                    key.CreditLimit = limit.GetDecimal();
                }
                
                if (data.TryGetProperty("rate_limit", out var rateLimit))
                {
                    if (rateLimit.TryGetProperty("requests", out var rpm))
                    {
                        key.Rpm = rpm.GetInt32();
                    }
                }

                if (data.TryGetProperty("is_free_tier", out var isFreeTier))
                {
                    key.BoughtCredits = !isFreeTier.GetBoolean();
                }

                // Calculate balance
                key.Balance = key.CreditLimit - key.Usage;
                key.LimitReached = key.Balance <= 0;
            }

            return new ValidationResult
            {
                IsValid = true,
                KeyInfo = key
            };
        }
        catch
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "OpenRouter validation error"
            };
        }
    }
}
