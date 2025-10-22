using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.VertexAI;

public class VertexAIValidator : BaseKeyValidator
{
    public override Provider Provider => Provider.VertexAI;

    public VertexAIValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            // Read service account JSON file
            if (!File.Exists(key.Key))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "VertexAI service account file not found"
                };
            }

            var serviceAccountJson = await File.ReadAllTextAsync(key.Key, cancellationToken);
            var serviceAccount = JsonSerializer.Deserialize<JsonElement>(serviceAccountJson);

            if (!serviceAccount.TryGetProperty("project_id", out var projectIdElement))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid service account JSON - missing project_id"
                };
            }

            var projectId = projectIdElement.GetString();
            key.ProjectId = projectId;

            // Get access token from service account
            var accessToken = await GetAccessTokenAsync(serviceAccount, cancellationToken);
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Failed to obtain access token"
                };
            }

            // Check Vertex AI access by listing models
            var modelsUrl = $"https://{GetRegion(projectId)}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{GetRegion(projectId)}/publishers/anthropic/models";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, modelsUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Unable to access Vertex AI - check permissions"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

            // Check if Claude Opus is available
            if (jsonResponse.TryGetProperty("models", out var models))
            {
                foreach (var model in models.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var modelName))
                    {
                        var name = modelName.GetString() ?? "";
                        if (name.Contains("opus", StringComparison.OrdinalIgnoreCase))
                        {
                            key.HasOpus = true;
                            break;
                        }
                    }
                }
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
                ErrorMessage = $"VertexAI validation error: {ex.Message}"
            };
        }
    }

    private async Task<string?> GetAccessTokenAsync(JsonElement serviceAccount, CancellationToken cancellationToken)
    {
        try
        {
            // Simplified OAuth2 flow - in production use Google.Apis.Auth library
            var tokenUrl = "https://oauth2.googleapis.com/token";
            
            if (!serviceAccount.TryGetProperty("client_email", out var clientEmail) ||
                !serviceAccount.TryGetProperty("private_key", out var privateKey))
            {
                return null;
            }

            // For simplicity, return a placeholder
            // In production, implement proper JWT signing with private key
            return "placeholder_token"; // This would be obtained via proper OAuth2 flow
        }
        catch
        {
            return null;
        }
    }

    private string GetRegion(string? projectId)
    {
        // Default to us-central1
        return "us-central1";
    }
}
