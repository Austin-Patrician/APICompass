namespace APICompass.KeyChecker.Validators.OpenAI;

public static class OpenAIModels
{
    public static readonly HashSet<string> StandardModelIds = new()
    {
        "omni-moderation-2024-09-26", "gpt-4o-mini-audio-preview-2024-12-17", "dall-e-3", "dall-e-2",
        "gpt-4o-audio-preview-2024-10-01", "o1", "gpt-4o-audio-preview", "gpt-4o-mini-realtime-preview-2024-12-17",
        "o1-2024-12-17", "gpt-4-0314", "gpt-4o-mini-realtime-preview", "o1-mini-2024-09-12",
        "o1-preview-2024-09-12", "o1-mini", "o1-preview", "gpt-4o-mini-audio-preview", "whisper-1",
        "gpt-4-turbo", "gpt-4o-realtime-preview-2024-10-01", "gpt-4", "babbage-002", "gpt-4-turbo-preview",
        "tts-1-hd-1106", "gpt-4-0125-preview", "gpt-4o-audio-preview-2024-12-17", "tts-1-hd",
        "gpt-4o-mini-2024-07-18", "gpt-4o-2024-08-06", "gpt-4o", "tts-1", "tts-1-1106",
        "gpt-4-turbo-2024-04-09", "davinci-002", "gpt-3.5-turbo-1106", "gpt-4o-mini", "gpt-4o-2024-05-13",
        "gpt-3.5-turbo-instruct", "chatgpt-4o-latest", "gpt-3.5-turbo-instruct-0914", "gpt-3.5-turbo-0125",
        "gpt-4o-realtime-preview-2024-12-17", "gpt-3.5-turbo", "gpt-3.5-turbo-16k-0613",
        "gpt-4o-realtime-preview", "gpt-3.5-turbo-16k", "text-embedding-3-small", "gpt-4o-2024-11-20",
        "gpt-4-1106-preview", "text-embedding-ada-002", "text-embedding-3-large", "o3-mini-2025-01-31",
        "gpt-4-0613", "o3-mini", "omni-moderation-latest", "gpt-4-base", "o1-pro", "o1-pro-2025-03-19",
        "gpt-4o-transcribe", "computer-use-preview", "computer-use-preview-2025-03-11", "gpt-4o-search-preview",
        "gpt-4o-search-preview-2025-03-11", "gpt-4o-mini-search-preview", "gpt-4o-mini-search-preview-2025-03-11",
        "gpt-4o-mini-transcribe", "gpt-4o-mini-tts", "o3", "o4-mini", "o3-2025-04-16", "o4-mini-2025-04-16",
        "gpt-4.1-mini", "gpt-4.1-mini-2025-04-14", "gpt-4.1-nano", "gpt-4.1-nano-2025-04-14", "gpt-4.1",
        "gpt-4.1-2025-04-14", "gpt-image-1", "codex-mini-latest", "gpt-4o-realtime-preview-2025-06-03",
        "gpt-4o-audio-preview-2025-06-03", "o3-pro", "o3-pro-2025-06-10", "o3-deep-research",
        "o3-deep-research-2025-06-26", "o4-mini-deep-research", "o4-mini-deep-research-2025-06-26",
        "gpt-5-mini", "gpt-5-mini-2025-08-07", "gpt-5-nano", "gpt-5-nano-2025-08-07", "gpt-5",
        "gpt-5-2025-08-07", "gpt-5-chat-latest", "gpt-audio-2025-08-28", "gpt-realtime-2025-08-28",
        "gpt-audio", "gpt-realtime"
    };

    public static readonly HashSet<string> NonSlopStandard = new()
    {
        "gpt-5", "gpt-5-chat-latest", "o3", "gpt-4.1", "chatgpt-4o-latest", "gpt-4o"
    };

    public static readonly Dictionary<string, int> TierRpmLimits = new()
    {
        { "Tier 1", 500 },
        { "Tier 2", 5000 },
        { "Tier 3", 5000 },
        { "Tier 4", 10000 },
        { "Tier 5", 15000 }
    };

    public static string GetTierFromRpm(int tpm)
    {
        return tpm switch
        {
            >= 40000000 => "Tier 5",
            >= 4000000 => "Tier 4",
            >= 2000000 => "Tier 3",
            >= 1000000 => "Tier 2",
            >= 500000 => "Tier 1",
            _ => "Free/Unknown"
        };
    }
}