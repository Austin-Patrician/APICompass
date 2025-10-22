namespace APICompass.KeyChecker.API.Models.Responses;

public class BatchValidationResponse
{
    public int TotalKeys { get; set; }
    public int ValidKeys { get; set; }
    public int InvalidKeys { get; set; }
    public List<ValidationResponse> Results { get; set; } = new();
    public double TotalDurationMs { get; set; }
}