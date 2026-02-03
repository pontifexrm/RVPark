using RVPark.Features.Authentication.DTOs;

namespace RVPark.Features.Authentication.Services;

public interface IAuthenticationService
{
    Task<AuthResultDto> LoginAsync(string email, string password, bool rememberMe);
    Task<AuthResultDto> RegisterAsync(string email, string password, string firstName, string lastName, string? mobile);
    Task LogoutAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}
