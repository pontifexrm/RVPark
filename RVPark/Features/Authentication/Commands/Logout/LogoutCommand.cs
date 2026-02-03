using MediatR;

namespace RVPark.Features.Authentication.Commands.Logout;

public record LogoutCommand() : IRequest<bool>;
