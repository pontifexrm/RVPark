using MediatR;
using RVPark.Features.Authentication.DTOs;
using RVPark.Features.Authentication.Services;

namespace RVPark.Features.Authentication.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IAuthenticationService _authService;

    public LoginHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);
    }
}
