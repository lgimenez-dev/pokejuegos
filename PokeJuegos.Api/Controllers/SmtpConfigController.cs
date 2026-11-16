using PokeJuegos.Api.Models;
using PokeJuegos.Application.SmtpConfigs;
using PokeJuegos.Domain;
using PokeJuegos.DTO;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PokeJuegos.Api.Controllers;

/// <summary>
/// Handles SMTP configuration endpoints under <c>/api/smtp-config</c>.
/// All endpoints require the <c>Admin</c> role.
/// </summary>
[ApiController]
[Route("api/smtp-config")]
[Authorize(Roles = "ADMIN")]
public class SmtpConfigController(
    ISmtpConfigService smtpConfigService,
    IValidator<CreateSmtpConfigRequest> createValidator,
    IValidator<UpdateSmtpConfigRequest> updateValidator) : ControllerBase
{
    /// <summary>
    /// Returns the current SMTP configuration.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="SmtpConfigResponse"/>; 404 Not Found if not configured yet.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        Result<SmtpConfigResponse> result = await smtpConfigService.GetAsync(ct);
        if (!result.IsSuccess)
        {
            return NotFound(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates the application's SMTP configuration. Only one record is allowed.
    /// </summary>
    /// <param name="request">The SMTP configuration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="SmtpConfigResponse"/> on success; 400 Bad Request on validation or business-rule failure.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSmtpConfigRequest request, CancellationToken ct)
    {
        ValidationResult validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<SmtpConfigResponse> result = await smtpConfigService.CreateAsync(request, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Replaces all fields of the existing SMTP configuration.
    /// </summary>
    /// <param name="request">The new SMTP configuration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="SmtpConfigResponse"/> on success; 400 on validation failure; 404 if not configured yet.</returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSmtpConfigRequest request, CancellationToken ct)
    {
        ValidationResult validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<SmtpConfigResponse> result = await smtpConfigService.UpdateAsync(request, ct);
        if (!result.IsSuccess)
        {
            return NotFound(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }
}
