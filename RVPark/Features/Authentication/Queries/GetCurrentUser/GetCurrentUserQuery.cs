using MediatR;
using RVPark.Features.Authentication.DTOs;

namespace RVPark.Features.Authentication.Queries.GetCurrentUser;

public record GetCurrentUserQuery() : IRequest<CurrentUserDto?>;
