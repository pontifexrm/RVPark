using RVPark.Data;

namespace RVPark.Features.Bookings.Services;

public interface IBookingEngine
{
    /// <summary>
    /// Checks if a booking request can be made (availability check only).
    /// </summary>
    bool RequestBooking(Bkg_Booking booking);

    /// <summary>
    /// Creates a new booking if availability exists for the requested dates.
    /// Updates availability records and creates the booking record in a transaction.
    /// </summary>
    Task<bool> CreateBookingAsync(Bkg_Booking booking);

    /// <summary>
    /// Edits an existing booking. Handles all 7 edit scenarios (A-G):
    /// A) New booking dates entirely before current dates
    /// B) New arrival before current, new departure within current period
    /// C) New booking spans entirely over current period
    /// D) New booking entirely within current period
    /// E) New arrival within current, new departure after current
    /// F) New booking dates entirely after current dates
    /// G) Property change (different property)
    /// </summary>
    Task<bool> EditBookingAsync(Bkg_Booking currentBooking, Bkg_Booking newBooking);

    /// <summary>
    /// Deletes a booking and resets availability records to available state.
    /// </summary>
    Task<bool> DeleteBookingAsync(Bkg_Booking booking);

    /// <summary>
    /// Checks if all dates in the booking period are available for the property.
    /// </summary>
    bool IsAvailable(Bkg_Booking booking);

    /// <summary>
    /// Checks if all dates in the specified range are available for the property.
    /// </summary>
    bool IsAvailable(DateTime fromDate, DateTime toDate, int propertyId);
}
