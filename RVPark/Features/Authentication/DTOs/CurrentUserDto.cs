namespace RVPark.Features.Authentication.DTOs;

public record CurrentUserDto(
    string UserId,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    IEnumerable<string> Roles,
    int? BkgUserId = null
);
