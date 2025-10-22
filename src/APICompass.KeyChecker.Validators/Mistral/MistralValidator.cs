using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.Mistral;

public class MistralValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://api.mistral.ai/v1";
    public override Provider Provider => Provider.Mistral;

    public MistralValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/models");
            request.Headers.Add("Authorization", $"Bearer {key.Key}");

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid Mistral key"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            // Check if user has subscription (inferred from available models)
            if (jsonResponse.TryGetProperty("data", out var models))
            {
                key.Subbed = models.GetArrayLength() > 0;
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
                ErrorMessage = "Mistral validation error"
            };
        }
    }
}
