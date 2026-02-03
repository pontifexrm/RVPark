namespace RVPark.Features.Authentication.DTOs;

public record AuthResultDto(
    bool Succeeded,
    string? UserId = null,
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    bool RequiresTwoFactor = false,
    bool IsLockedOut = false,
    bool RequiresEmailConfirmation = false,
    string? ErrorMessage = null,
    IEnumerable<string>? Errors = null
);
