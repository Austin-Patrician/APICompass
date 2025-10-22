using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Core.Interfaces;

public interface IKeyValidator
{
    Provider Provider { get; }
    Task<ValidationResult> ValidateAsync(APIKey key, ValidationOptions options, CancellationToken cancellationToken = default);
}
