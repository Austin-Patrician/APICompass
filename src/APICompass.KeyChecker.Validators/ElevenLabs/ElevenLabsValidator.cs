using System.Net.Http.Json;
using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using APICompass.KeyChecker.Validators.Base;

namespace APICompass.KeyChecker.Validators.ElevenLabs;

public class ElevenLabsValidator : BaseKeyValidator
{
    private const string ApiUrl = "https://api.elevenlabs.io/v1";
    public override Provider Provider => Provider.ElevenLabs;

    public ElevenLabsValidator(HttpClient httpClient, IValidationCache cache, SemaphoreSlim semaphore)
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/user/subscription");
            request.Headers.Add("xi-api-key", key.Key);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid ElevenLabs key"
                };
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            if (jsonResponse.TryGetProperty("tier", out var tier))
            {
                key.Tier = tier.GetString() ?? "Free";
            }

            if (jsonResponse.TryGetProperty("character_count", out var charCount))
            {
                key.CharactersLeft = charCount.GetInt64();
            }

            if (jsonResponse.TryGetProperty("character_limit", out var charLimit))
            {
                var limit = charLimit.GetInt64();
                key.Unlimited = limit < 0 || limit > 10000000;
            }

            if (jsonResponse.TryGetProperty("professional_voice_limit", out var voiceLimit))
            {
                key.ProVoiceLimit = voiceLimit.GetInt32();
            }

            key.ElevenLabsUsage = $"{key.CharactersLeft} characters remaining";

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
                ErrorMessage = "ElevenLabs validation error"
            };
        }
    }
}
