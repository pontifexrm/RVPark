using MediatR;
using RVPark.Features.Authentication.DTOs;
using RVPark.Features.Authentication.Services;

namespace RVPark.Features.Authentication.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IAuthenticationService _authService;

    public RegisterHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Mobile
        );
    }
}
