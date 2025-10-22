namespace APICompass.KeyChecker.Core.Configuration;

public class ValidationSettings
{
    public int MaxConcurrentValidations { get; set; } = 10000;
    public int DefaultTimeoutSeconds { get; set; } = 30;
    public bool EnableCaching { get; set; } = true;
    public int CacheDurationSeconds { get; set; } = 300;
    public Dictionary<string, ProviderSettings> Providers { get; set; } = new();
}

public class ProviderSettings
{
    public int MaxConcurrent { get; set; } = 1500;
    public int RetryCount { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 30;
}
