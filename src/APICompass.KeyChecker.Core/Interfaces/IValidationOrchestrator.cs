using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Core.Interfaces;

public interface IValidationOrchestrator
{
    Task<ValidationResult> ValidateSingleAsync(string key, ValidationOptions? options = null, CancellationToken cancellationToken = default);
    Task<List<ValidationResult>> ValidateBatchAsync(IEnumerable<string> keys, ValidationOptions? options = null, CancellationToken cancellationToken = default);
}
