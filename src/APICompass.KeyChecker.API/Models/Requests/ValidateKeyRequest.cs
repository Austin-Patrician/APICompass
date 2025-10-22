using System.ComponentModel.DataAnnotations;

namespace APICompass.KeyChecker.API.Models.Requests;

public class ValidateKeyRequest
{
    [Required]
    public string Key { get; set; } = string.Empty;

    public string? Provider { get; set; }

    public ValidationOptionsRequest? Options { get; set; }
}

public class ValidationOptionsRequest
{
    public bool VerifyOrg { get; set; }
    public bool CheckModels { get; set; } = true;
    public bool UseCache { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}
