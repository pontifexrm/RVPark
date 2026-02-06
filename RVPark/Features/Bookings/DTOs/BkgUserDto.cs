namespace RVPark.Features.Bookings.DTOs;

/// <summary>
/// DTO representing a booking user (Bkg_User).
/// </summary>
public record BkgUserDto(
    int UserId,
    string UserName,
    string? UserFirstName,
    string? UserLastName,
    string? UserEmail,
    string? UserPhone
);
