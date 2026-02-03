using MediatR;
using RVPark.Features.Authentication.Services;

namespace RVPark.Features.Authentication.Commands.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthenticationService _authService;

    public LogoutHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync();
        return true;
    }
}
