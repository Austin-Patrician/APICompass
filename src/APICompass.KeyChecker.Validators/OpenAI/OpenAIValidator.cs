using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.OpenAI;

public class OpenAIValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://api.openai.com/v1";
    public override Provider Provider => Provider.OpenAI;

    public OpenAIValidator(
        HttpClient httpClient,
        IValidationCache cache,
        SemaphoreSlim semaphore)
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
            // Step 1: Get available models
            var modelCheckSuccess = await GetModelsAsync(key, cancellationToken);
            if (!modelCheckSuccess)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Unable to retrieve models - invalid key"
                };
            }

            // Step 2: Get key attributes (quota, tier, rate limits)
            var attributesSuccess = await GetKeyAttributesAsync(key, options, cancellationToken);
            if (!attributesSuccess)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Unable to verify key attributes"
                };
            }

            // Step 3: Get organizations (if applicable)
            await GetOrganizationsAsync(key, cancellationToken);

            return new ValidationResult
            {
                IsValid = true,
                KeyInfo = key
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"OpenAI validation error: {ex.Message}"
            };
        }
    }

    private async Task<bool> GetModelsAsync(APIKey key, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Key);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                // Key is valid but doesn't have access to model listing
                key.Model = "gpt-5";
                key.AccessToModelListing = false;
                return true;
            }

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var models = jsonResponse.GetProperty("data").EnumerateArray();
            
            var accessibleModels = new HashSet<string>();

            foreach (var model in models)
            {
                var modelId = model.GetProperty("id").GetString();
                if (string.IsNullOrEmpty(modelId)) continue;

                // Check for fine-tuned models
                if (modelId.Contains("ft:"))
                {
                    key.HasSpecialModels = true;
                }
                // Check for extra models not in standard list
                else if (!OpenAIModels.StandardModelIds.Contains(modelId) && !modelId.Contains(":ft-"))
                {
                    key.ExtraModels = true;
                    key.ExtraModelList.Add(modelId);
                }

                // Check for special rare models
                if (modelId == "gpt-4-base" || modelId == "gpt-5-alpha-max" || modelId == "gpt-4.5-preview")
                {
                    key.TheOne = true;
                    key.Slop = false;
                }

                if (modelId == "gpt-4-32k" || modelId == "gpt-4-32k-0613")
                {
                    key.Real32K = true;
                    key.Slop = false;
                }

                // Check for non-slop standard models
                if (OpenAIModels.NonSlopStandard.Contains(modelId))
                {
                    key.Slop = false;
                    key.Model = modelId;
                    accessibleModels.Add(modelId);
                }
            }

            key.MissingModels = OpenAIModels.NonSlopStandard.Except(accessibleModels).ToHashSet();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> GetKeyAttributesAsync(APIKey key, ValidationOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var model = string.IsNullOrEmpty(key.Model) ? "gpt-4o" : key.Model;
            var param = model.Contains("gpt-4") ? "max_tokens" : "max_completion_tokens";

            var chatRequest = new
            {
                model,
                messages = new[] { new { role = "user", content = "" } },
                max_completion_tokens = 0
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Key);
            request.Content = JsonContent.Create(chatRequest);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return true; // Valid key but restricted
            }

            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == (HttpStatusCode)429)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                var errorType = jsonResponse.GetProperty("error").GetProperty("type").GetString();

                switch (errorType)
                {
                    case "access_terminated":
                    case "billing_not_active":
                        return false;
                    
                    case "insufficient_quota":
                        key.HasQuota = false;
                        break;
                    
                    case "invalid_request_error":
                        key.HasQuota = true;
                        
                        // Extract rate limits from headers
                        if (response.Headers.TryGetValues("x-ratelimit-limit-requests", out var rpmValues))
                        {
                            key.Rpm = int.Parse(rpmValues.First());
                        }
                        
                        if (response.Headers.TryGetValues("x-ratelimit-limit-tokens", out var tpmValues))
                        {
                            var tpm = int.Parse(tpmValues.First());
                            key.Tier = OpenAIModels.GetTierFromRpm(tpm);
                        }

                        // Optional ID verification
                        if (options.VerifyOrg)
                        {
                            key.IdVerified = await CheckIdVerifiedAsync(key, cancellationToken);
                        }

                        key.Slop = false;
                        break;
                }

                return true;
            }

            return !response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.BadGateway;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckIdVerifiedAsync(APIKey key, CancellationToken cancellationToken)
    {
        try
        {
            var chatRequest = new
            {
                model = "o3",
                messages = new[] { new { role = "user", content = "" } },
                max_completion_tokens = 1,
                stream = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Key);
            request.Content = JsonContent.Create(chatRequest);

            using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return !errorContent.Contains("organization must complete");
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task GetOrganizationsAsync(APIKey key, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/organizations");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Key);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode) return;

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            if (jsonResponse.TryGetProperty("data", out var data))
            {
                foreach (var org in data.EnumerateArray())
                {
                    var orgId = org.GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(orgId))
                    {
                        key.Organizations.Add(orgId);
                    }

                    if (org.TryGetProperty("is_default", out var isDefault) && isDefault.GetBoolean())
                    {
                        key.DefaultOrg = orgId;
                    }
                }
            }
        }
        catch
        {
            // Organizations are optional, don't fail validation
        }
    }
}
