using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.AWS;

public class AWSValidator : BaseKeyValidator
{
    public override Provider Provider => Provider.AWS;

    public AWSValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
                    ErrorMessage = "Invalid AWS key format. Expected AKIA...:secret"
                };
            }

            var accessKey = parts[0];
            var secretKey = parts[1];

            // Try to determine region by calling STS GetCallerIdentity (works in all regions)
            var region = "us-east-1"; // Default region
            key.Region = region;

            // Call STS GetCallerIdentity to validate credentials
            var stsResult = await CallStsGetCallerIdentityAsync(accessKey, secretKey, region, cancellationToken);
            
            if (!stsResult.isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid AWS credentials"
                };
            }

            key.Username = stsResult.username;
            key.Useless = false;

            // Check IAM permissions
            var hasAdmin = await CheckIAMPermissionsAsync(accessKey, secretKey, region, cancellationToken);
            key.AdminPriv = hasAdmin;

            // Check Bedrock access
            var bedrockStatus = await CheckBedrockAccessAsync(accessKey, secretKey, region, cancellationToken);
            key.BedrockEnabled = bedrockStatus.enabled;
            if (bedrockStatus.models != null)
            {
                key.Models = bedrockStatus.models;
            }

            if (!key.BedrockEnabled)
            {
                key.UselessReasons.Add("Bedrock not enabled");
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
                ErrorMessage = $"AWS validation error: {ex.Message}"
            };
        }
    }

    private async Task<(bool isValid, string? username)> CallStsGetCallerIdentityAsync(
        string accessKey,
        string secretKey,
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"https://sts.{region}.amazonaws.com/";
            var service = "sts";
            var action = "GetCallerIdentity";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Action", action },
                { "Version", "2011-06-15" }
            });

            SignAwsRequest(request, accessKey, secretKey, region, service);

            var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Parse XML response to get username/ARN
            var arnStart = content.IndexOf("<Arn>") + 5;
            var arnEnd = content.IndexOf("</Arn>");
            if (arnStart > 5 && arnEnd > arnStart)
            {
                var arn = content.Substring(arnStart, arnEnd - arnStart);
                var username = arn.Split('/').LastOrDefault() ?? "Unknown";
                return (true, username);
            }

            return (true, "Unknown");
        }
        catch
        {
            return (false, null);
        }
    }

    private async Task<bool> CheckIAMPermissionsAsync(
        string accessKey,
        string secretKey,
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user can list IAM users (admin-level permission)
            var endpoint = $"https://iam.amazonaws.com/";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Action", "ListUsers" },
                { "Version", "2010-05-08" },
                { "MaxItems", "1" }
            });

            SignAwsRequest(request, accessKey, secretKey, region, "iam");

            var response = await HttpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<(bool enabled, Dictionary<string, object>? models)> CheckBedrockAccessAsync(
        string accessKey,
        string secretKey,
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"https://bedrock.{region}.amazonaws.com/foundation-models";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            SignAwsRequest(request, accessKey, secretKey, region, "bedrock");

            var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var models = new Dictionary<string, object>();

            if (jsonResponse.TryGetProperty("modelSummaries", out var modelSummaries))
            {
                foreach (var model in modelSummaries.EnumerateArray())
                {
                    if (model.TryGetProperty("modelId", out var modelId))
                    {
                        models[modelId.GetString() ?? ""] = true;
                    }
                }
            }

            return (true, models);
        }
        catch
        {
            return (false, null);
        }
    }

    private void SignAwsRequest(
        HttpRequestMessage request,
        string accessKey,
        string secretKey,
        string region,
        string service)
    {
        var dateTime = DateTime.UtcNow;
        var dateStamp = dateTime.ToString("yyyyMMdd");
        var amzDate = dateTime.ToString("yyyyMMddTHHmmssZ");

        request.Headers.Add("X-Amz-Date", amzDate);
        
        // Simplified AWS Signature V4 signing
        // In production, use AWS SDK or a complete implementation
        var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
        var authorization = $"AWS4-HMAC-SHA256 Credential={accessKey}/{credentialScope}";
        
        request.Headers.Add("Authorization", authorization);
    }
}
