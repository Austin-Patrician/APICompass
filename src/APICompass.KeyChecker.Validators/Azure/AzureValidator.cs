using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.Azure;

public class AzureValidator : BaseKeyValidator
{
    public override Provider Provider => Provider.Azure;

    public AzureValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            var parts = key.Key.Split(':');
            if (parts.Length != 2)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid Azure key format. Expected endpoint:apikey"
                };
            }

            var endpoint = parts[0];
            var apiKey = parts[1];

            key.Endpoint = endpoint;

            // Ensure endpoint is a full URL
            if (!endpoint.StartsWith("http"))
            {
                endpoint = $"https://{endpoint}.openai.azure.com";
            }

            // Get deployments
            var deploymentsUrl = $"{endpoint}/openai/deployments?api-version=2023-05-15";
            using var request = new HttpRequestMessage(HttpMethod.Get, deploymentsUrl);
            request.Headers.Add("api-key", apiKey);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid Azure OpenAI credentials"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

            if (jsonResponse.TryGetProperty("data", out var deployments))
            {
                string? bestModel = null;
                var highestPriority = 0;

                foreach (var deployment in deployments.EnumerateArray())
                {
                    if (deployment.TryGetProperty("id", out var id))
                    {
                        var deploymentName = id.GetString() ?? "";
                        key.Deployments.Add(deploymentName);

                        // Check model
                        if (deployment.TryGetProperty("model", out var model))
                        {
                            var modelName = model.GetString() ?? "";
                            
                            // Priority: GPT-4 > GPT-4 Turbo > GPT-3.5
                            var priority = modelName.ToLower() switch
                            {
                                var m when m.Contains("gpt-4") && m.Contains("turbo") => 3,
                                var m when m.Contains("gpt-4") => 2,
                                var m when m.Contains("gpt-3.5") => 1,
                                _ => 0
                            };

                            if (priority > highestPriority)
                            {
                                highestPriority = priority;
                                bestModel = modelName;
                                key.BestDeployment = deploymentName;
                            }

                            if (modelName.Contains("gpt-4") && modelName.Contains("turbo"))
                            {
                                key.HasGpt4Turbo.Add(deploymentName);
                            }
                        }
                    }
                }

                key.Model = bestModel;
            }

            // Check for content filter (simple check - try a request)
            if (!string.IsNullOrEmpty(key.BestDeployment))
            {
                key.Unfiltered = await CheckContentFilterAsync(endpoint, apiKey, key.BestDeployment, cancellationToken);
            }

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
                ErrorMessage = $"Azure validation error: {ex.Message}"
            };
        }
    }

    private async Task<bool> CheckContentFilterAsync(
        string endpoint,
        string apiKey,
        string deployment,
        CancellationToken cancellationToken)
    {
        try
        {
            var chatUrl = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2023-05-15";
            
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = "Hello" }
                },
                max_tokens = 1
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, chatUrl);
            request.Headers.Add("api-key", apiKey);
            request.Content = JsonContent.Create(requestBody);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            // If we get a content filter error, it's filtered
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return !error.Contains("content_filter");
            }

            return true; // No content filter detected
        }
        catch
        {
            return false;
        }
    }
}
