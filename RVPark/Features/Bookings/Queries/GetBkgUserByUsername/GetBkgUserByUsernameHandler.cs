using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetBkgUserByUsername;

public class GetBkgUserByUsernameHandler : IRequestHandler<GetBkgUserByUsernameQuery, BkgUserDto?>
{
    private readonly ApplicationDbContext _context;

    public GetBkgUserByUsernameHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BkgUserDto?> Handle(GetBkgUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.bkg_Users
            .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);

        if (user == null)
            return null;

        return new BkgUserDto(
            UserId: user.UserId,
            UserName: user.UserName,
            UserFirstName: user.UserFirstName,
            UserLastName: user.UserLastName,
            UserEmail: user.UserEmail,
            UserPhone: user.UserPhone
        );
    }
}
