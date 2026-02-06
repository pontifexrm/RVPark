using MediatR;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetBkgUserByUsername;

/// <summary>
/// Query to get a Bkg_User by their username (email).
/// </summary>
public record GetBkgUserByUsernameQuery(string Username) : IRequest<BkgUserDto?>;
