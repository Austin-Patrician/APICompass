namespace APICompass.KeyChecker.Core.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public APIKey? KeyInfo { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ValidationDuration { get; set; }
}
