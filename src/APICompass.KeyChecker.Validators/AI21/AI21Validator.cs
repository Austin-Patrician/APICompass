using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.AI21;

public class AI21Validator : BaseKeyValidator
{
    private const string ApiUrl = "https://api.ai21.com/studio/v1";
    public override Provider Provider => Provider.AI21;

    public AI21Validator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/account");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Key);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid AI21 key"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            // Check if trial has elapsed
            if (jsonResponse.TryGetProperty("trial_elapsed", out var trialElapsed))
            {
                key.TrialElapsed = trialElapsed.GetBoolean();
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
                ErrorMessage = "AI21 validation error"
            };
        }
    }
}
