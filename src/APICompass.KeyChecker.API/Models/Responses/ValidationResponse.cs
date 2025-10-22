namespace APICompass.KeyChecker.API.Models.Responses;

public class ValidationResponse
{
    public bool IsValid { get; set; }
    public string? Provider { get; set; }
    public KeyInfoResponse? KeyInfo { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ValidatedAt { get; set; }
    public double ValidationDurationMs { get; set; }
}

public class KeyInfoResponse
{
    public string? Model { get; set; }
    public bool HasQuota { get; set; }
    public string? Tier { get; set; }
    public int Rpm { get; set; }
    public List<string> Organizations { get; set; } = new();
    public bool HasSpecialModels { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}
