using MediatR;
using RVPark.Features.Authentication.DTOs;

namespace RVPark.Features.Authentication.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false
) : IRequest<AuthResultDto>;
