using MediatR;
using RVPark.Features.Authentication.DTOs;
using RVPark.Features.Authentication.Services;

namespace RVPark.Features.Authentication.Queries.GetCurrentUser;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    private readonly IAuthenticationService _authService;

    public GetCurrentUserHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetCurrentUserAsync();
    }
}
