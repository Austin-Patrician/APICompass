using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.MakerSuite;

public class MakerSuiteValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta";
    public override Provider Provider => Provider.MakerSuite;

    public MakerSuiteValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/models?key={key.Key}");

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid MakerSuite key"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            if (jsonResponse.TryGetProperty("models", out var models))
            {
                foreach (var model in models.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var name))
                    {
                        key.MakerSuiteModels.Add(name.GetString() ?? "");
                    }
                }
            }

            // Extract tier from rate limit headers if available
            if (response.Headers.TryGetValues("x-goog-quota-user", out var quotaValues))
            {
                key.Tier = "Standard";
            }
            else
            {
                key.Tier = "Free";
            }

            key.EnabledBilling = key.MakerSuiteModels.Count > 3;

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
                ErrorMessage = "MakerSuite validation error"
            };
        }
    }
}
