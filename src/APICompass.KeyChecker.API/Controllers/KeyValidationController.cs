using System.Diagnostics;
using APICompass.KeyChecker.API.Models.Requests;
using APICompass.KeyChecker.API.Models.Responses;
using APICompass.KeyChecker.Core.Interfaces;
using APICompass.KeyChecker.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace APICompass.KeyChecker.API.Controllers;

[ApiController]
[Route("api/v1/keys")]
public class KeyValidationController : ControllerBase
{
    private readonly IValidationOrchestrator _orchestrator;
    private readonly ILogger<KeyValidationController> _logger;

    public KeyValidationController(
        IValidationOrchestrator orchestrator,
        ILogger<KeyValidationController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateKey(
        [FromBody] ValidateKeyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var options = MapToValidationOptions(request.Options);
            var result = await _orchestrator.ValidateSingleAsync(request.Key, options, cancellationToken);

            var response = MapToValidationResponse(result);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating key");
            return BadRequest(new { error = "Validation error occurred" });
        }
    }

    [HttpPost("validate/batch")]
    [ProducesResponseType(typeof(BatchValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateBatch(
        [FromBody] BatchValidateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var options = MapToValidationOptions(request.Options);

            var results = await _orchestrator.ValidateBatchAsync(request.Keys, options, cancellationToken);
            stopwatch.Stop();

            var response = new BatchValidationResponse
            {
                TotalKeys = results.Count,
                ValidKeys = results.Count(r => r.IsValid),
                InvalidKeys = results.Count(r => !r.IsValid),
                Results = results.Select(MapToValidationResponse).ToList(),
                TotalDurationMs = stopwatch.Elapsed.TotalMilliseconds
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating batch");
            return BadRequest(new { error = "Batch validation error occurred" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    private static ValidationOptions MapToValidationOptions(ValidationOptionsRequest? request)
    {
        if (request == null)
            return new ValidationOptions();

        return new ValidationOptions
        {
            VerifyOrg = request.VerifyOrg,
            CheckModels = request.CheckModels,
            UseCache = request.UseCache,
            TimeoutSeconds = request.TimeoutSeconds
        };
    }

    private static ValidationResponse MapToValidationResponse(ValidationResult result)
    {
        return new ValidationResponse
        {
            IsValid = result.IsValid,
            Provider = result.KeyInfo?.Provider.ToString(),
            ErrorMessage = result.ErrorMessage,
            ValidatedAt = result.ValidatedAt,
            ValidationDurationMs = result.ValidationDuration.TotalMilliseconds,
            KeyInfo = result.KeyInfo != null ? MapToKeyInfoResponse(result.KeyInfo) : null
        };
    }

    private static KeyInfoResponse MapToKeyInfoResponse(APIKey key)
    {
        var info = new KeyInfoResponse
        {
            Model = key.Model,
            HasQuota = key.HasQuota,
            Tier = key.Tier,
            Rpm = key.Rpm,
            Organizations = key.Organizations,
            HasSpecialModels = key.HasSpecialModels
        };

        // Add provider-specific additional info
        switch (key.Provider)
        {
            case Provider.OpenAI:
                info.AdditionalInfo["slop"] = key.Slop;
                info.AdditionalInfo["real32K"] = key.Real32K;
                info.AdditionalInfo["theOne"] = key.TheOne;
                info.AdditionalInfo["idVerified"] = key.IdVerified;
                if (key.ExtraModels)
                    info.AdditionalInfo["extraModels"] = key.ExtraModelList;
                break;

            case Provider.Anthropic:
                info.AdditionalInfo["pozzed"] = key.Pozzed;
                info.AdditionalInfo["rateLimited"] = key.RateLimited;
                info.AdditionalInfo["remainingTokens"] = key.RemainingTokens;
                break;

            case Provider.AWS:
                info.AdditionalInfo["username"] = key.Username ?? "";
                info.AdditionalInfo["bedrockEnabled"] = key.BedrockEnabled;
                info.AdditionalInfo["region"] = key.Region ?? "";
                break;

            case Provider.Azure:
                info.AdditionalInfo["endpoint"] = key.Endpoint ?? "";
                info.AdditionalInfo["bestDeployment"] = key.BestDeployment ?? "";
                info.AdditionalInfo["unfiltered"] = key.Unfiltered;
                break;
        }

        return info;
    }
}