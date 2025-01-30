using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TextTemplating;
using RVParking.Components.Pages;
using System;
using TNZAPI.NET.Api.Addressbook.Group.Dto;

namespace RVParking.Data
{
    /* This class is the engine for the booking system. It is used to request, create, change, and cancel/delete bookings. 
     * It also has utility methods to check availability and change availability status. 
     * It is used in the Blazor pages to interact with the database. 
     * 
     * Bkg_Engine.docx has more information on how to use this class and is the definitive source of information.
     * 
     * Bkg_Availablity is the class that holds the availability status of a property for a given date.
     * and is used to determine if a booking can be made or changed.
     * Adding and Deleting bookings is straight forward with either a test to see if the booking can be made
     * and when deleting returning the deleted bookigs availability records to a state they can be used for other bookings.
     * When changing a booking the availability records are updated to reflect the new booking dates. and there are tests to see if the new booking can be made.
     * The scenarios for a current and new booking with the booking engine are: (current A1,D1,P1 and new A2,D2,P2 A-arrive, D-depart, P-property)
     *                          A1----------D1
     * (a)     A2----------D2    |           |
     * (b)                A2----------D2     |
     * (c)             A2---------------------------D2
     * (d)                       | A2----D2  |
     * (e)                       |      A2----------D2
     * (f)                       |           |   A2----------D2
     * (g)      P1 != P2 Like two different booking New is a New one and if OK then delete the old one.
     * This engine Handles All These Scenarios
     */
    public class Bkg_Engine
    {
        #region Defn and Ctor and Injections
        private readonly ApplicationDbContext _context;
        private Bkg_Availability? Bkg_Availability;
        private Bkg_Booking? Bkg_Booking;
        private Bkg_Property? Bkg_Property;
        private Bkg_Booking? curr_Booking; // current booking that is being changed
        private Bkg_Booking? new_Booking; // booking that an existing booking is to be changed to

        public Bkg_Engine(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region Top level methods for using the booking engine in other blazor pages.
        public bool RequestBooking(Bkg_Booking currentBooking)
        {
            bool result = false;
            if (IsAvailable(currentBooking))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// This method is used to DELETE the Requested Booking first resetting the associated availability and  status of its property.
        /// It is transaction safe in that if the delete fails the availability and status of the property is not changed.
        /// And other processes can not use the availability records during the time it runs.
        /// </summary>
        /// <param name="currentBooking"></param>
        /// <returns> True or false </returns>
        public async Task<bool> DeleteBookingAsync(Bkg_Booking currentBooking)
        {

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // for each Bkg_availability record that is part of the current booking
                    // set the availability to true and the status to "Available" and Stats to string.Empty
                    var dte = currentBooking.DateArrive;
                    while (dte <= currentBooking.DateArrive)
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        var avail = _context.bkg_Availabilities
                            .Where(a => a.Bkg_PropertyId == currentBooking.PropertyId && a.DateAvailable == dte)
                            .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.
                        if (avail == null)
                        {
                            throw new InvalidOperationException($"Availability record not found for property {currentBooking.PropertyId} on date {dte}");
                        }
                        else
                        {
                            avail.Available = true;
                            avail.AvailabilityStatus = string.Empty;
                            _context.bkg_Availabilities.Update(avail); 
                        }
                        dte = dte.AddDays(1);
                    }
                    // remove the booking
                    _context.bkg_Bookings?.Remove(currentBooking);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            
        }

        /// <summary>
        /// This method is used to CREATE a new Booking if the availability status of a property for it are all true.
        /// needs to have a begin commit transaction and a try catch block to handle any errors.
        /// determines if it can create the booking by calling isAvailable then chages the state of those availability records.
        /// finally creating the neccessary booking records.
        /// </summary>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        public async Task<bool> CreateBookingAsync(Bkg_Booking newBooking)
        {
            bool result = false;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (IsAvailable(newBooking))
                    {
                        // for each Bkg_availability record that is part of the new booking
                        // set the availability to false and the status to "Booked" and Stats to string.Empty
                        var dte = newBooking.DateArrive;
                        while (dte < newBooking.DateDepart)
                        {
                            var avail = _context.bkg_Availabilities?
                                .Where(a => a.Bkg_PropertyId == newBooking.PropertyId && a.DateAvailable == dte)
                                .FirstOrDefault();
                            if (avail == null)
                            {
                                throw new InvalidOperationException($"Availability record not found for property {newBooking.PropertyId} on date {dte}");
                            }
                            else
                            {
                                avail.Available = false;
                                avail.AvailabilityStatus = "Booked";
                                //_ = _context.bkg_Availabilities?.Update(avail!);
                                _context.bkg_Availabilities?.Update(avail);
                            }
                            dte = dte.AddDays(1);
                        }
                        // add the booking
                        newBooking.BookingStatus = "Booked";
                        _context.bkg_Bookings?.Add(newBooking);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        result = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            return result;
        }

        /// <summary>
        /// EditBookingAsync() is used to change a booking. It is a complex method that needs to be transaction safe.
        /// As well as take into account the availability status of the possible a new property and new booking dates.
        /// There are 65 scenarios for a current and new booking Edit Feature referring to
        /// (current A1, D1, P1 and new A2,D2,P2 A-arrive, D-depart, P-property)
        ///                        A1----------D1
        /// (a) A2----------D2     |           |
        /// (b)             A2----------D2     |           
        /// (c) A2---------------------------D2
        /// (d)                    | A2----D2  |
        /// (e)                    |      A2----------D2
        /// (f)                    |           |   A2----------D2
        /// 
        /// </summary>
        /// <param name="cur_bkg"></param>
        /// <param name="new_bkg"></param>
        /// <returns></returns>
        public async Task<bool> EditBookingAsync(Bkg_Booking cur_bkg, Bkg_Booking new_bkg)
        {
            bool result = false;
            // Scenario A  Both A2 and D2 are before A1 P1 == P2
            if (new_bkg.DateArrive < cur_bkg.DateArrive && new_bkg.DateDepart < cur_bkg.DateArrive
                && cur_bkg.PropertyId == new_bkg.PropertyId)
            { // we need to check if the new booking is available and if so change the availability status of the property
              // and then update the booking record. reset the Availability records of the old booking date range.
              // if the new booking is not available then we return false.
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(new_bkg))
                        {
                            chngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
                            _context.bkg_Bookings?.Update(new_bkg);
                            chngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            }// Scenario B A2 is before A1 and D2 is Between A1 and D1,  P1 == P2
            else if (new_bkg.DateArrive < cur_bkg.DateArrive && new_bkg.DateDepart >= cur_bkg.DateArrive && new_bkg.DateDepart <= cur_bkg.DateDepart
                        && cur_bkg.PropertyId == new_bkg.PropertyId)
            {   // Check if the new booking is available between A2 and A1 and if so change the availability status of the property
                // and then update the booking record. reset the Availability records between D2 and D1 of the old booking date range.
                // if the new booking is not available then we return false.
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId))
                        {
                            chngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
                            chngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
                            _context.bkg_Bookings?.Update(new_bkg);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            }  // Scenario C  A2 is before A1 and D2 is after D1 P1 == P2
            else if (new_bkg.DateArrive < cur_bkg.DateArrive && new_bkg.DateDepart > cur_bkg.DateDepart
                     && cur_bkg.PropertyId == new_bkg.PropertyId)
            {   // Check if new_Bkg is available between A2 and A1 as well as between D1 and D2  
                // If so change the availability status of the property and then update the booking record.
                // If no exit FALSE
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId) && IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId))
                        {
                            chngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive, new_bkg.PropertyId, false, "Booked");
                            chngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
                            _context.bkg_Bookings?.Update(new_bkg);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            }// Scenario D  A2 and D2 are both between A1 and D1,  P1 == P2
            else if (new_bkg.DateArrive >= cur_bkg.DateArrive && new_bkg.DateArrive < cur_bkg.DateDepart && new_bkg.DateDepart <= cur_bkg.DateDepart
                 && cur_bkg.PropertyId == new_bkg.PropertyId)
            {   // The new_Bkg is between A1 and D1 so is already available
                // so we change the booking and reset before A2 and after D1 
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        chngAvailableStatus(new_bkg.DateArrive, cur_bkg.DateArrive,  cur_bkg.PropertyId, true, string.Empty);
                        chngAvailableStatus(new_bkg.DateDepart, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
                        _context.bkg_Bookings?.Update(new_bkg);
                        await _context.SaveChangesAsync();
                        result = true;
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            } // Scenario E A2 is between A1 and D1 and D2 is after D1,  P1 == P2
            else if (new_bkg.DateArrive >= cur_bkg.DateArrive && new_bkg.DateArrive < cur_bkg.DateDepart && new_bkg.DateDepart > cur_bkg.DateDepart
                 && cur_bkg.PropertyId == new_bkg.PropertyId)
            {   // Check if availability is OK between D1 and D2 and if so change the availability status of the property for that range
                // and then update the booking record. reset the Availability records between A1 and A1 of the old booking date range.
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId))
                        {
                            chngAvailableStatus(cur_bkg.DateArrive, new_bkg.DateArrive, new_bkg.PropertyId, true, string.Empty);
                            chngAvailableStatus(cur_bkg.DateDepart, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
                            _context.bkg_Bookings?.Update(new_bkg);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            } // Scenario F  A2 and D2 are both after D1,  P1 == P2
            else if (new_bkg.DateArrive >= cur_bkg.DateDepart && new_bkg.DateDepart > cur_bkg.DateDepart
                 && cur_bkg.PropertyId == new_bkg.PropertyId)
            {   // Check if available between A2 and D2 and if so change the availability status of the property for that range
                // and then update the booking record. reset the Availability records between A1 and D1 of the old booking date range.
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(new_bkg))
                        {
                            chngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
                            chngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
                            _context.bkg_Bookings.Update(new_bkg);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            return result;
            } // Scenario G   P1 != P2
            else if (cur_bkg.PropertyId != new_bkg.PropertyId)
            {   // Check if the new booking is available and if so change the availability status of the property
                // and then update the booking record. reset the Availability records of the old booking date range.
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (IsAvailable(new_bkg))
                        {
                            chngAvailableStatus(cur_bkg.DateArrive, cur_bkg.DateDepart, cur_bkg.PropertyId, true, string.Empty);
                            chngAvailableStatus(new_bkg.DateArrive, new_bkg.DateDepart, new_bkg.PropertyId, false, "Booked");
                            _context.bkg_Bookings?.Update(new_bkg);
                            await _context.SaveChangesAsync();
                            result = true;
                            transaction.Commit();
                        }
                        else
                        {
                            result = false;
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return result;
            } // we have Not catered for the case where the change is NOT covered above so need to throw and error
            else
            {
                result = false;
            }
            return result;
        }



        #endregion

        #region Utility methods for use inside the Bkg_Engine class
        // these methods are for the booking engine to use when creating or changing a booking

        public bool IsAvailable(Bkg_Booking bkg)
        {
            bool result = false;
            var dte = bkg.DateArrive;
            while (dte < bkg.DateDepart)
            {
                var avail = _context.bkg_Availabilities?
                    .Where(a => a.Bkg_PropertyId == bkg.PropertyId && a.DateAvailable == dte)
                    .FirstOrDefault();
                if (avail != null)
                {
                    if (avail.Available)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    result = false;
                    break;
                }
                dte = dte.AddDays(1);
            }
            return result;
        }
        public bool IsAvailable(DateTime fmdte, DateTime todte, int propId)
        {
            bool result = false;
            var dte = fmdte;
            while (dte < todte)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                var avail = _context.bkg_Availabilities
                    .Where(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte)
                    .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.
                if (avail != null)
                {
                    if (avail.Available)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    result = false;
                    break;
                }
                dte = dte.AddDays(1);
            }
            return result;

        }
        public bool chngAvailableStatus(DateTime fmdte, DateTime todte, int propId, bool isavail, string status)
        {
            bool result = true;
            DateTime dte = fmdte;
            while (dte < todte)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                var existingRecord = _context.bkg_Availabilities
                    .FirstOrDefault(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte);
#pragma warning restore CS8604 // Possible null reference argument.

                if (existingRecord != null)
                {
                    existingRecord.Available = isavail;
                    existingRecord.AvailabilityStatus = status;
                }
                else
                {
                    result = false;
                    break;
                }
                dte = dte.AddDays(1);
            }
            return result;
        }

        #endregion


        #region        // Copilot Example code NOT USED

        /// <summary>
        /// structure for a master detail transaction. NOT USED
        /// </summary>
        /// <param name="bkg"></param>
        /// <returns></returns>
        //public async Task<bool> TransCommandsExample(Bkg_Something something)
        //{
        //    bool result = false;
        //    using (var context = _context)
        //    {
        //        using (var transaction = context.Database.BeginTransaction())
        //        {
        //            try
        //            {

        //         do Cammand loop on detail(sbyte)
        //var dte = something.DateArrive;
        //while (dte <= something.DateArrive)
        //{
        //    var avail = _context.bkg_Availabilities
        //        .Where(a => a.Bkg_PropertyId == something.PropertyId && a.DateAvailable == dte)
        //        .FirstOrDefault();
        //    if (avail == null)
        //    {
        //        throw new InvalidOperationException($"Availability record not found for property {something.PropertyId} on date {dte}");
        //    }
        //    else
        //    {
        //        avail.Available = true;
        //        avail.AvailabilityStatus = string.Empty;
        //        _ = context.bkg_Availabilities?.Update(avail!);
        //    }
        //    dte = dte.AddDays(1);
        //}
        // do whatever to the master
        //...                        
        //await context.SaveChangesAsync();
        //transaction.Commit();
        //return true;
        //}
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            return false;
        //        }
        //    }
        //}
        //return result;

        //      }





        // utility methods


        /// <summary>
        /// ChangeBookingAsync() is used to change a booking. Basically produced by Copilot as a sample  NOT used.See EditBookingAsync() for the correct method.
        /// </summary>
        /// <param name="currentBooking"></param>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        public async Task<bool> ChangeBookingAsync(Bkg_Booking currentBooking, Bkg_Booking newBooking)
        {
            bool result = false;
            if (IsAvailable(newBooking))
            {
                _context.bkg_Bookings.Update(newBooking);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }
        /// <summary>
        /// CancelBookingSync() is used to cancel a booking. Basically produced by Copilot as a sample  NOT used.See DeleteBookingAsync() for the correct method.
        /// /// </summary>
        /// <param name="currentBooking"></param>
        /// <returns></returns>
        public async Task<bool> CancelBookingAsync(Bkg_Booking currentBooking)
        {
            bool result = false;
            _context.bkg_Bookings.Remove(currentBooking);
            await _context.SaveChangesAsync();
            result = true;
            return result;
        }
        /// <summary>
        /// This method is used to Request a new Booking to see if the availability status of a property for a given date range is OK or not.
        /// </summary>
        /// <param name="currentBooking"></param>
        /// <returns>
        ///     True  if currentBooking is possible
        ///     False if currentBooking is NOT possible
        /// </returns>

        // Example method that uses the DbContext
        public async Task<List<Bkg_Property>> GetPropertiesAsync()
        {
            return await (_context.bkg_Properties ?? throw new InvalidOperationException("bkg_Properties is null")).ToListAsync();
        }
        #endregion
    }
}
