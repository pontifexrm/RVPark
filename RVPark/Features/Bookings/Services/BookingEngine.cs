using RVPark.Data;

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

    public BookingEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public bool RequestBooking(Bkg_Booking currentBooking)
    {
        return IsAvailable(currentBooking);
    }

    /// <summary>
    /// Deletes the requested booking, first resetting the associated availability status.
    /// Transaction-safe: if the delete fails, availability is not changed.
    /// </summary>
    public async Task<bool> DeleteBookingAsync(Bkg_Booking currentBooking)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var dte = currentBooking.DateArrive;
            while (dte < currentBooking.DateDepart)
            {
                var avail = _context.bkg_Availabilities!
                    .Where(a => a.Bkg_PropertyId == currentBooking.PropertyId && a.DateAvailable == dte)
                    .FirstOrDefault();

                if (avail == null)
                {
                    throw new InvalidOperationException(
                        $"Availability record not found for property {currentBooking.PropertyId} on date {dte}");
                }

                avail.Available = true;
                avail.AvailabilityStatus = string.Empty;
                _context.bkg_Availabilities!.Update(avail);
                dte = dte.AddDays(1);
            }

            _context.bkg_Bookings!.Remove(currentBooking);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    /// <summary>
    /// Creates a new booking if availability status of a property for the dates are all true.
    /// Transaction-safe with proper availability record updates.
    /// </summary>
    public async Task<bool> CreateBookingAsync(Bkg_Booking newBooking)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(newBooking))
            {
                transaction.Rollback();
                return false;
            }

            var dte = newBooking.DateArrive.Date;
            while (dte < newBooking.DateDepart)
            {
                var avail = _context.bkg_Availabilities!
                    .Where(a => a.Bkg_PropertyId == newBooking.PropertyId && a.DateAvailable == dte)
                    .FirstOrDefault();

                if (avail == null)
                {
                    throw new InvalidOperationException(
                        $"Availability record not found for property {newBooking.PropertyId} on date {dte}");
                }

                avail.Available = false;
                avail.AvailabilityStatus = "Booked";
                _context.bkg_Availabilities!.Update(avail);
                dte = dte.AddDays(1);
            }

            newBooking.BookingStatus = "Booked";
            _context.bkg_Bookings!.Add(newBooking);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    /// <summary>
    /// Handles all 7 edit scenarios (A-G) for booking changes.
    /// </summary>
    public async Task<bool> EditBookingAsync(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
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
        return false;
    }

    #region Edit Scenario Handlers

    private async Task<bool> HandleScenarioA(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(new_bkg))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            _context.bkg_Bookings!.Update(new_bkg);
            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioB(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
            ChngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioC(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId)
                || !IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
            ChngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioD(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            ChngAvailableStatus(cur_bkg.DateArrive, new_bkg.DateArrive, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioE(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(cur_bkg.DateArrive, new_bkg.DateArrive, new_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioF(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(new_bkg))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    private async Task<bool> HandleScenarioG(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsAvailable(new_bkg))
            {
                transaction.Rollback();
                return false;
            }

            ChngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
            ChngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
            _context.bkg_Bookings!.Update(new_bkg);
            await _context.SaveChangesAsync();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    #endregion

    #region Utility Methods

    public bool IsAvailable(Bkg_Booking bkg)
    {
        var dte = bkg.DateArrive.Date;
        while (dte < bkg.DateDepart)
        {
            var avail = _context.bkg_Availabilities!
                .Where(a => a.Bkg_PropertyId == bkg.PropertyId && a.DateAvailable == dte)
                .FirstOrDefault();

            if (avail == null || !avail.Available)
            {
                return false;
            }

            dte = dte.AddDays(1);
        }
        return true;
    }

    public bool IsAvailable(DateTime fmdte, DateTime todte, int propId)
    {
        var dte = fmdte;
        while (dte < todte)
        {
            var avail = _context.bkg_Availabilities!
                .Where(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte)
                .FirstOrDefault();

            if (avail == null || !avail.Available)
            {
                return false;
            }

            dte = dte.AddDays(1);
        }
        return true;
    }

    private bool ChngAvailableStatus(DateTime fmdte, DateTime todte, int propId, bool isavail, string status)
    {
        var dte = fmdte;
        while (dte < todte)
        {
            var existingRecord = _context.bkg_Availabilities!
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
