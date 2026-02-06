using RVPark.Data;
using RVPark.Services.Logging;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Services;

/// <summary>
/// This class is the engine for the booking system. It is used to request, create, change, and cancel/delete bookings.
/// It also has utility methods to check availability and change availability status.
///
/// The scenarios for a current and new booking with the booking engine are: (current A1,D1,P1 and new A2,D2,P2 A-arrive, D-depart, P-property)
///                         A1----------D1
/// (a)     A2----------D2   |           |
/// (b)             A2----------D2       |
/// (c)          A2---------------------------D2
/// (d)                      | A2----D2  |
/// (e)                      |      A2----------D2
/// (f)                      |           |   A2----------D2
/// (g)      P1 != P2 Like two different booking New is a New one and if OK then delete the old one.
/// </summary>
public class BookingEngine : IBookingEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogger _logger;

    public BookingEngine(ApplicationDbContext context, IAppLogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public Result<bool> RequestBooking(Bkg_Booking currentBooking)
    {
        return IsAvailable(currentBooking);
    }

    /// <summary>
    /// Deletes the requested booking, first resetting the associated availability status.
    /// Transaction-safe: if the delete fails, availability is not changed.
    /// </summary>
    public async Task<Result> DeleteBookingAsync(Bkg_Booking currentBooking)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var dte = currentBooking.DateArrive;
            while (dte < currentBooking.DateDepart)
            {
                var avail = _context.bkg_Availabilities
                    .Where(a => a.Bkg_PropertyId == currentBooking.PropertyId && a.DateAvailable == dte)
                    .FirstOrDefault();

                if (avail == null)
                {
                    await _logger.LogAsync("Error", "BookingEngine.DeleteBookingAsync",
                        $"Availability record not found for property {currentBooking.PropertyId} on date {dte:dd/MM/yyyy}");
                    transaction.Rollback();
                    return Result.Failure($"Availability record not found for property {currentBooking.PropertyId} on date {dte:dd/MM/yyyy}");
                }

                avail.Available = true;
                avail.AvailabilityStatus = string.Empty;
                _context.bkg_Availabilities.Update(avail);
                dte = dte.AddDays(1);
            }

            _context.bkg_Bookings.Remove(currentBooking);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.DeleteBookingAsync",
                $"Booking {currentBooking.BookingId} deleted for property {currentBooking.PropertyId}, dates {currentBooking.DateArrive:dd/MM/yyyy} to {currentBooking.DateDepart:dd/MM/yyyy}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.DeleteBookingAsync",
                $"Failed to delete booking {currentBooking.BookingId}: {ex.Message}", ex);
            return Result.Failure("An unexpected error occurred while deleting the booking. Please try again.");
        }
    }

    /// <summary>
    /// Creates a new booking if availability status of a property for the dates are all true.
    /// Transaction-safe with proper availability record updates.
    /// </summary>
    public async Task<Result<Bkg_Booking>> CreateBookingAsync(Bkg_Booking newBooking)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(newBooking);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available.");
            }

            var dte = newBooking.DateArrive.Date;
            while (dte < newBooking.DateDepart)
            {
                var avail = _context.bkg_Availabilities
                    .Where(a => a.Bkg_PropertyId == newBooking.PropertyId && a.DateAvailable == dte)
                    .FirstOrDefault();

                if (avail == null)
                {
                    await _logger.LogAsync("Error", "BookingEngine.CreateBookingAsync",
                        $"Availability record not found for property {newBooking.PropertyId} on date {dte:dd/MM/yyyy}");
                    transaction.Rollback();
                    return Result<Bkg_Booking>.Failure($"Availability record not found for property {newBooking.PropertyId} on date {dte:dd/MM/yyyy}");
                }

                avail.Available = false;
                avail.AvailabilityStatus = "Booked";
                _context.bkg_Availabilities.Update(avail);
                dte = dte.AddDays(1);
            }

            newBooking.BookingStatus = "Booked";
            _context.bkg_Bookings.Add(newBooking);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.CreateBookingAsync",
                $"Booking {newBooking.BookingId} created for property {newBooking.PropertyId}, dates {newBooking.DateArrive:dd/MM/yyyy} to {newBooking.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(newBooking);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.CreateBookingAsync",
                $"Failed to create booking: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while creating the booking. Please try again.");
        }
    }

    /// <summary>
    /// Handles all 7 edit scenarios (A-G) for booking changes.
    /// </summary>
    public async Task<Result<Bkg_Booking>> EditBookingAsync(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        // Scenario A: Both A2 and D2 are before A1 (P1 == P2)
        if (new_bkg.DateArrive < cur_bkg.DateArrive && new_bkg.DateDepart < cur_bkg.DateArrive
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioA(cur_bkg, new_bkg);
        }

        // Scenario B: A2 is before A1 and D2 is between A1 and D1 (P1 == P2)
        if (new_bkg.DateArrive < cur_bkg.DateArrive
            && new_bkg.DateDepart >= cur_bkg.DateArrive
            && new_bkg.DateDepart <= cur_bkg.DateDepart
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioB(cur_bkg, new_bkg);
        }

        // Scenario C: A2 is before A1 and D2 is after D1 (P1 == P2)
        if (new_bkg.DateArrive < cur_bkg.DateArrive && new_bkg.DateDepart > cur_bkg.DateDepart
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioC(cur_bkg, new_bkg);
        }

        // Scenario D: A2 and D2 are both between A1 and D1 (P1 == P2)
        if (new_bkg.DateArrive >= cur_bkg.DateArrive
            && new_bkg.DateArrive < cur_bkg.DateDepart
            && new_bkg.DateDepart <= cur_bkg.DateDepart
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioD(cur_bkg, new_bkg);
        }

        // Scenario E: A2 is between A1 and D1 and D2 is after D1 (P1 == P2)
        if (new_bkg.DateArrive >= cur_bkg.DateArrive
            && new_bkg.DateArrive < cur_bkg.DateDepart
            && new_bkg.DateDepart > cur_bkg.DateDepart
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioE(cur_bkg, new_bkg);
        }

        // Scenario F: A2 and D2 are both after D1 (P1 == P2)
        if (new_bkg.DateArrive >= cur_bkg.DateDepart && new_bkg.DateDepart > cur_bkg.DateDepart
            && cur_bkg.PropertyId == new_bkg.PropertyId)
        {
            return await HandleScenarioF(cur_bkg, new_bkg);
        }

        // Scenario G: P1 != P2 (property change)
        if (cur_bkg.PropertyId != new_bkg.PropertyId)
        {
            return await HandleScenarioG(cur_bkg, new_bkg);
        }

        // Not covered by any scenario
        await _logger.LogAsync("Warning", "BookingEngine.EditBookingAsync",
            $"Edit scenario not recognized for booking {cur_bkg.BookingId}");
        return Result<Bkg_Booking>.Failure("Unable to process the booking change. The date combination is not supported.");
    }

    #region Edit Scenario Handlers

    private async Task<Result<Bkg_Booking>> HandleScenarioA(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(new_bkg);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available.");
            }

            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            UpdateBookingEntity(new_bkg);
            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario A) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioA",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioB(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available.");
            }

            ChngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
            ChngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario B) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioB",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioC(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var beforeResult = IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId);
            var afterResult = IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId);

            if (!beforeResult.IsSuccess || !beforeResult.Value || !afterResult.IsSuccess || !afterResult.Value)
            {
                transaction.Rollback();
                var errorMsg = beforeResult.Error ?? afterResult.Error ?? "The requested dates are not available.";
                return Result<Bkg_Booking>.Failure(errorMsg);
            }

            ChngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
            ChngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario C) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioC",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioD(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            ChngAvailableStatus(cur_bkg.DateArrive, new_bkg.DateArrive, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario D) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioD",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioE(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available.");
            }

            ChngAvailableStatus(cur_bkg.DateArrive, new_bkg.DateArrive, new_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario E) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioE",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioF(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(new_bkg);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available.");
            }

            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario F) for property {new_bkg.PropertyId}, new dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioF",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    private async Task<Result<Bkg_Booking>> HandleScenarioG(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var availabilityResult = IsAvailable(new_bkg);
            if (!availabilityResult.IsSuccess || !availabilityResult.Value)
            {
                transaction.Rollback();
                return Result<Bkg_Booking>.Failure(availabilityResult.Error ?? "The requested dates are not available on the new property.");
            }

            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            UpdateBookingEntity(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();

            await _logger.LogAsync("Info", "BookingEngine.EditBookingAsync",
                $"Booking {new_bkg.BookingId} edited (Scenario G - property change) from property {cur_bkg.PropertyId} to {new_bkg.PropertyId}, dates {new_bkg.DateArrive:dd/MM/yyyy} to {new_bkg.DateDepart:dd/MM/yyyy}");

            return Result<Bkg_Booking>.Success(new_bkg);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await _logger.LogAsync("Error", "BookingEngine.HandleScenarioG",
                $"Failed to edit booking {new_bkg.BookingId}: {ex.Message}", ex);
            return Result<Bkg_Booking>.Failure("An unexpected error occurred while editing the booking. Please try again.");
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Safely updates a booking entity, handling EF Core tracking conflicts.
    /// </summary>
    private void UpdateBookingEntity(Bkg_Booking new_bkg)
    {
        var trackedEntity = _context.ChangeTracker.Entries<Bkg_Booking>()
            .FirstOrDefault(e => e.Entity.BookingId == new_bkg.BookingId);

        if (trackedEntity != null)
        {
            // Update the tracked entity's properties
            trackedEntity.CurrentValues.SetValues(new_bkg);
        }
        else
        {
            // No tracked entity, safe to attach and update
            UpdateBookingEntity(new_bkg);
        }
    }

    public Result<bool> IsAvailable(Bkg_Booking bkg)
    {
        var unavailableDates = new List<string>();
        var dte = bkg.DateArrive.Date;
        while (dte < bkg.DateDepart)
        {
            var avail = _context.bkg_Availabilities
                .Where(a => a.Bkg_PropertyId == bkg.PropertyId && a.DateAvailable == dte)
                .FirstOrDefault();

            if (avail == null || !avail.Available)
            {
                unavailableDates.Add(dte.ToString("dd/MM/yyyy"));
            }

            dte = dte.AddDays(1);
        }

        if (unavailableDates.Count > 0)
        {
            var dateList = string.Join(", ", unavailableDates);
            return Result<bool>.Failure($"The following dates are not available: {dateList}");
        }

        return Result<bool>.Success(true);
    }

    public Result<bool> IsAvailable(DateTime fmdte, DateTime todte, int propId)
    {
        var unavailableDates = new List<string>();
        var dte = fmdte;
        while (dte < todte)
        {
            var avail = _context.bkg_Availabilities
                .Where(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte)
                .FirstOrDefault();

            if (avail == null || !avail.Available)
            {
                unavailableDates.Add(dte.ToString("dd/MM/yyyy"));
            }

            dte = dte.AddDays(1);
        }

        if (unavailableDates.Count > 0)
        {
            var dateList = string.Join(", ", unavailableDates);
            return Result<bool>.Failure($"The following dates are not available: {dateList}");
        }

        return Result<bool>.Success(true);
    }

    private bool ChngAvailableStatus(DateTime fmdte, DateTime todte, int propId, bool isavail, string status)
    {
        var dte = fmdte;
        while (dte < todte)
        {
            var existingRecord = _context.bkg_Availabilities
                .FirstOrDefault(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte);

            if (existingRecord == null)
            {
                return false;
            }

            existingRecord.Available = isavail;
            existingRecord.AvailabilityStatus = status;
            dte = dte.AddDays(1);
        }
        return true;
    }

    #endregion
}
