namespace APICompass.KeyChecker.Core.Models;

public class ValidationOptions
{
    public bool VerifyOrg { get; set; }
    public bool CheckModels { get; set; } = true;
    public bool UseCache { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 5;
}
