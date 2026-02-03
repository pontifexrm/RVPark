using MediatR;
using RVPark.Features.Authentication.DTOs;

namespace RVPark.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? Mobile = null
) : IRequest<AuthResultDto>;
