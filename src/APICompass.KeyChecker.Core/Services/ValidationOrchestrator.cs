using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Core.Services;

public class ValidationOrchestrator : IValidationOrchestrator
{
    private readonly IKeyIdentifierService _keyIdentifier;
    private readonly IEnumerable<IKeyValidator> _validators;
    private readonly Dictionary<Provider, IKeyValidator> _validatorMap;

    public ValidationOrchestrator(
        IKeyIdentifierService keyIdentifier,
        IEnumerable<IKeyValidator> validators)
    {
        _keyIdentifier = keyIdentifier;
        _validators = validators;
        _validatorMap = validators.ToDictionary(v => v.Provider);
    }

    public async Task<ValidationResult> ValidateSingleAsync(
        string key,
        ValidationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ValidationOptions();

        // Identify provider
        var (provider, isValid) = _keyIdentifier.IdentifyProvider(key);

        if (!isValid || provider == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Unable to identify key provider"
            };
        }

        // Get appropriate validator
        if (!_validatorMap.TryGetValue(provider.Value, out var validator))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"No validator available for provider: {provider.Value}"
            };
        }

        // Create APIKey object
        var apiKey = new APIKey
        {
            Provider = provider.Value,
            Key = key
        };

        // Validate
        return await validator.ValidateAsync(apiKey, options, cancellationToken);
    }

    public async Task<List<ValidationResult>> ValidateBatchAsync(
        IEnumerable<string> keys,
        ValidationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ValidationOptions();

        var tasks = keys.Select(key => ValidateSingleAsync(key, options, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results.ToList();
    }
}
