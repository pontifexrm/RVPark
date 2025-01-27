using Microsoft.EntityFrameworkCore;
using System;

namespace RVParking.Data
{
    public class Bkg_Engine
    {
        private readonly ApplicationDbContext _context;

        public Bkg_Engine(ApplicationDbContext context)
        {
            _context = context;
        }

        // Example method that uses the DbContext
        public async Task<List<Bkg_Property>> GetPropertiesAsync()
        {
            return await _context.bkg_Properties.ToListAsync();
        }
        public bool isAvailable(Bkg_Booking currentBooking)
        {
            bool result = false;
            //if (currentBooking.BookingId == 0)
            //{
            //    // a new booking so see if available
            //    var dte = currentBooking.DateArrive;
            //    while (dte <= currentBooking.DateDepart)
            //    {
            //        var avail = _context.bkg_Availabilities
            //            .Where(a => a.Bkg_PropertyId == currentBooking.PropertyId && a.DateAvailable == dte)
            //            .FirstOrDefault();
            //        if (avail != null)
            //        {
            //            if (avail.Available)
            //            {
            //                result = true;
            //            }
            //            else
            //            {
            //                result = false;
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            result = false;
            //            break;
            //        }
            //        dte = dte.AddDays(1);
            //    }
            //}
            //else
            //{
            //    _context.bkg_Bookings.Update(currentBooking);
            //}

            //var isAvail = false;

            return result;
        }
        public bool isAvailable(DateTime fmdte, DateTime todte, int propId)
        {
            bool result = false;
            var dte = fmdte;
            while (dte <= todte)
            {
                var avail = _context.bkg_Availabilities
                    .Where(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte)
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
        public bool chngAvailableStatus(DateTime fmdte, DateTime todte, int propId, bool isavail, string status)
        {
            bool result = true;
            DateTime dte = fmdte;
            while (dte <= todte)
            {
                var existingRecord = _context.bkg_Availabilities
                    .FirstOrDefault(a => a.Bkg_PropertyId == propId && a.DateAvailable == dte);

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

        // these methods are for the booking engine to use when creating or changing a booking




    }
}
