namespace RVPark.Features.Bookings.DTOs;

public record BookingDto(
    int BookingId,
    int PropertyId,
    int UserId,
    DateTime DateArrive,
    DateTime DateDepart,
    decimal TotalPrice,
    bool Paid,
    string BookingStatus,
    string? BookingComments,
    DateTime CreatedDte,
    DateTime UpdatedDte,
    int Nights,
    // Related data for display
    string? PropertyName = null,
    string? UserFirstName = null,
    string? UserLastName = null,
    string? UserName = null,
    string? UserEmail = null,
    string? UserPhone = null
);
