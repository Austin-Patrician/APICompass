using System.ComponentModel.DataAnnotations;

namespace APICompass.KeyChecker.API.Models.Requests;

public class BatchValidateRequest
{
    [Required]
    [MinLength(1)]
    public List<string> Keys { get; set; } = new();

    public ValidationOptionsRequest? Options { get; set; }
}
