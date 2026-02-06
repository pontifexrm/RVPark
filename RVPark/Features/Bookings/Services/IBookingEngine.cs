using RVPark.Data;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Services;

public interface IBookingEngine
{
    /// <summary>
    /// Checks if a booking request can be made (availability check only).
    /// Returns Result with unavailable dates in error message if not available.
    /// </summary>
    Result<bool> RequestBooking(Bkg_Booking booking);

    /// <summary>
    /// Creates a new booking if availability exists for the requested dates.
    /// Updates availability records and creates the booking record in a transaction.
    /// Returns Result with the created booking or error message.
    /// </summary>
    Task<Result<Bkg_Booking>> CreateBookingAsync(Bkg_Booking booking);

    /// <summary>
    /// Edits an existing booking. Handles all 7 edit scenarios (A-G):
    /// A) New booking dates entirely before current dates
    /// B) New arrival before current, new departure within current period
    /// C) New booking spans entirely over current period
    /// D) New booking entirely within current period
    /// E) New arrival within current, new departure after current
    /// F) New booking dates entirely after current dates
    /// G) Property change (different property)
    /// Returns Result with the updated booking or error message.
    /// </summary>
    Task<Result<Bkg_Booking>> EditBookingAsync(Bkg_Booking currentBooking, Bkg_Booking newBooking);

    /// <summary>
    /// Deletes a booking and resets availability records to available state.
    /// Returns Result indicating success or error message.
    /// </summary>
    Task<Result> DeleteBookingAsync(Bkg_Booking booking);

    /// <summary>
    /// Checks if all dates in the booking period are available for the property.
    /// Returns Result with unavailable dates in error message if not available.
    /// </summary>
    Result<bool> IsAvailable(Bkg_Booking booking);

    /// <summary>
    /// Checks if all dates in the specified range are available for the property.
    /// Returns Result with unavailable dates in error message if not available.
    /// </summary>
    Result<bool> IsAvailable(DateTime fromDate, DateTime toDate, int propertyId);
}
