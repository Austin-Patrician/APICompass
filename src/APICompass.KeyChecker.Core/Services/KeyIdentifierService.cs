using System.Text.RegularExpressions;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Core.Services;

public partial class KeyIdentifierService : IKeyIdentifierService
{
    [GeneratedRegex(@"(sk-[a-zA-Z0-9_-]+T3BlbkFJ[a-zA-Z0-9_-]+)")]
    private static partial Regex OpenAIRegex();
    
    [GeneratedRegex(@"sk-ant-api03-[A-Za-z0-9\-_]{93}AA")]
    private static partial Regex AnthropicPrimaryRegex();
    
    [GeneratedRegex(@"sk-ant-[A-Za-z0-9\-_]{86}")]
    private static partial Regex AnthropicSecondaryRegex();
    
    [GeneratedRegex(@"sk-[A-Za-z0-9]{86}")]
    private static partial Regex AnthropicThirdRegex();
    
    [GeneratedRegex(@"[A-Za-z0-9]{32}")]
    private static partial Regex AI21AndMistralRegex();
    
    [GeneratedRegex(@"([a-z0-9]{32})")]
    private static partial Regex ElevenLabsRegex();
    
    [GeneratedRegex(@"sk_[a-z0-9]{48}")]
    private static partial Regex ElevenLabsSecondaryRegex();
    
    [GeneratedRegex(@"AIzaSy[A-Za-z0-9\-_]{33}")]
    private static partial Regex MakerSuiteRegex();
    
    [GeneratedRegex(@"^(AKIA[0-9A-Z]{16}):([A-Za-z0-9+/]{40})$")]
    private static partial Regex AWSRegex();
    
    [GeneratedRegex(@"^(.+):([a-z0-9]{32})$")]
    private static partial Regex AzureRegex();
    
    [GeneratedRegex(@"sk-or-v1-[a-z0-9]{64}")]
    private static partial Regex OpenRouterRegex();
    
    [GeneratedRegex(@"sk-[a-f0-9]{32}")]
    private static partial Regex DeepSeekRegex();
    
    [GeneratedRegex(@"xai-[A-Za-z0-9]{80}")]
    private static partial Regex XAIRegex();

    public (Provider? provider, bool isValid) IdentifyProvider(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (null, false);

        key = key.Trim().Trim('"');

        // Check for VertexAI (file path)
        if (key.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && File.Exists(key))
        {
            return (Provider.VertexAI, true);
        }

        // Check for Anthropic (multiple patterns)
        if (key.StartsWith("sk-ant-", StringComparison.Ordinal))
        {
            if (key.Contains("api03") && AnthropicPrimaryRegex().IsMatch(key))
                return (Provider.Anthropic, true);
            if (AnthropicSecondaryRegex().IsMatch(key))
                return (Provider.Anthropic, true);
        }

        // Check for MakerSuite
        if (key.StartsWith("AIzaSy", StringComparison.Ordinal) && MakerSuiteRegex().IsMatch(key))
        {
            return (Provider.MakerSuite, true);
        }

        // Check for XAI
        if (key.StartsWith("xai-", StringComparison.Ordinal) && XAIRegex().IsMatch(key))
        {
            return (Provider.XAI, true);
        }

        // Check for OpenRouter
        if (key.StartsWith("sk-or-v1-", StringComparison.Ordinal) && OpenRouterRegex().IsMatch(key))
        {
            return (Provider.OpenRouter, true);
        }

        // Check for sk- prefixed keys (OpenAI, Anthropic alternative, DeepSeek)
        if (key.StartsWith("sk-", StringComparison.Ordinal))
        {
            // DeepSeek (shorter length)
            if (key.Length < 36 && DeepSeekRegex().IsMatch(key))
                return (Provider.DeepSeek, true);

            // Anthropic alternative pattern (longer than OpenAI)
            if (key.Length > 36 && !key.Contains("T3BlbkFJ") && AnthropicThirdRegex().IsMatch(key))
                return (Provider.Anthropic, true);

            // OpenAI
            if (OpenAIRegex().IsMatch(key))
                return (Provider.OpenAI, true);
        }

        // Check for AWS (AKIA prefix with colon separator)
        if (key.Contains(':') && key.Contains("AKIA", StringComparison.Ordinal) && AWSRegex().IsMatch(key))
        {
            return (Provider.AWS, true);
        }

        // Check for Azure (colon separator but not AWS)
        if (key.Contains(':') && !key.Contains("AKIA", StringComparison.Ordinal) && AzureRegex().IsMatch(key))
        {
            return (Provider.Azure, true);
        }

        // Check for ElevenLabs
        if (key.StartsWith("sk_", StringComparison.Ordinal) && ElevenLabsSecondaryRegex().IsMatch(key))
        {
            return (Provider.ElevenLabs, true);
        }

        if (ElevenLabsRegex().IsMatch(key))
        {
            return (Provider.ElevenLabs, true);
        }

        // Check for AI21/Mistral (32 character generic keys)
        if (AI21AndMistralRegex().IsMatch(key) && key.Length == 32)
        {
            return (Provider.AI21, true); // Will try Mistral as fallback during validation
        }

        return (null, false);
    }
}
