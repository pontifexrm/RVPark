using Azure;
//using Humanizer;
//using Microsoft.CodeAnalysis.Differencing;
using RVParking.Components.Account.Pages.Manage;
using RVParking.Components.Pages;
using RVParking.Components;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;


namespace RVParking.Data;

public class EmailMessage
{
    public EmailMessage()
    {
    }

    [Required, EmailAddress,MaxLength(48)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

   // [Required, MinLength(3), MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(128)]
    public string Name { get; set; } = string.Empty;


   // [Required, MinLength(3), MaxLength(128)]
    public string LastName { get; set; } = string.Empty;
    
    [Required, MinLength(4), MaxLength(80)]
    public string Subject { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(128)]
    public string Message { get; set; } = string.Empty;
    [Required, MaxLength(32)]
    public string Nzmca { get; set; } = string.Empty;

    public Bkg_User bkg_User { get; set; } = new Bkg_User();

    public DateTime Fmdate { get; set; } = DateTime.Now.Date;

    public DateTime Todate { get; set; } = DateTime.Now.Date;
    public string FullName
    {
        get
        {
            return string.Concat(bkg_User.UserFirstName, " ", bkg_User.UserLastName);
        }
    }

    public string  E_164_Phone  //IntPhoneFmt //The proper E.164 format is [+] [country code] [area code] [subscriber number].
    {
        get
        {
            // purpose of this return is to gie the phone number in the +64nnnnnnnnnn format if it is not already.
            string srtn = string.Empty;
            if(this.Phone.Length > 0)
            {
                if (Phone.StartsWith("+")) // then likely in the corect format already
                {
                    srtn = Phone.Trim();
                }
                else if(Phone.StartsWith("0"))// then likely miss country code so remove the "0" and add "+64"
                {
                    srtn = string.Concat("+64" + Phone[1..]); // this prefix should be a configurable for the ountry the app is being run in.
                }
            }
            return srtn;
        }
    }
    public string Chk_E_164_Phone(string phne)
    {
        string srtn = string.Empty;
        if (phne.Length > 0)
        {
            if (phne.StartsWith("+")) // then likely in the corect format already
            {
                srtn = phne.Trim();
            }
            else if (phne.StartsWith("0"))// then likely miss country code so remove the "0" and add "+64"
            {
                srtn = string.Concat("+64" + phne[1..]); // this prefix should be a configurable for the ountry the app is being run in.
            }
        }
        return srtn;
    }
    public string ChkMsg
    { 
        get{
            string sRtn = string.Format("Hi {0} \r\n" +
                              "Thanks for your interest in RV Park NZ.\r\n\r\nWe have received your message and will be contacting you soon with our response. \r\n" +
                              "Just so you have a record of this message here are the details you sent to us. \r\n\r\n" +
                              "SUBJECT: [{7}]\r\n" +
                              "Your email address is [{1}] \r\n" +
                              "Your Phone number is [{2}] \r\n" +
                              "You are interested in dates from [{3}] to [{4}] \r\n" +
                              "If you are an NZMCA member your number being [{5}] \r\n" +
                              "And finally your message is \r\n[{6}] \r\n\r\n" +
                              "Many thanks \r\n" +
                              "RV Park on Fitz \r\n" +
                              "Date: {8}",
            this.Name, this.Email, this.Phone,
            this.Fmdate.ToString("yyyy-MM-dd"), this.Todate.ToString("yyyy-MM-dd"), this.Nzmca,
            this.Message, this.Subject, DateTime.Now.ToString("g"));
            return sRtn;
        } 
    }
    public string smsMsg
    {
        get
        {
            string sRtn = string.Format("RVPark fm: {0} \r\n" +                              
                              "SUBJECT: {3}  - {4}\r\n" +
                              "email {1} \r\n" +
                              "Phone {2} \r\n" +
                              "Dates {6} to {7} \r\n" +
                              "{5}",
            this.Name, this.Email, this.E_164_Phone,
            this.Subject, this.Message, DateTime.Now.ToString("g"),
            this.Fmdate.ToString("yyyy-MM-dd"), this.Todate.ToString("yyyy-MM-dd"));
            return sRtn;
        }
    }

    public string ConfirmBkgMsg
    {
        get
        {
            string sRtn = string.Format("Hi {0} ," +
                "\r\n" +
            "Thanks for your interest in RV Park on Fitz.\r\nThis email confirms your booking with the details as you will see below.\r\n" +
            "Booking for: [{5}] \r\n" +
            "        Arrival {1} after mid-day and departing {2} before mid-day.\r\n" +
            "Address is\r\n" +
            "        22 Fitzpatrick St\r\n" +
            "        Newlands\r\n" +
            "        Wellington 6037\r\n" +
            "Access is best from Stewart Drive.Either from the north where you would exit SH1 at Johnsonville or from the South turning into Newlands. Turn into Fitzpatrick St from Stewart Drive is best as other routes(courtesy of the map apps) send you around a very narrow road.We are on the left corner of the 2nd street on the left going up Fitzpatrick St.\r\n" +
            "Payment is $30 for the night with NZMCA Memebrship {3}, payable in advance please to our bank account.\r\n" +
            "        Ronald Pontifex\r\n" +
            "        03 0525 0253043 000\r\n" +
            "(We have Wise Card if that suits you better.)\r\n\r\n" +
            "Please text us on arrival and we shall come out to meets you etc.\r\n" +
            "Again many thanks\r\n" +
            "Ron Pontifex\r\n" +
            "WELLINGTON New Zealand\r\n" +
            "Ph 027 304 2002\r\n" +
            "E - Mail : rvpark@pontifex.nz\r\n" +
             "Date: {6}",
            this.FullName, this.Fmdate.ToString("yyyy-MM-dd"), this.Todate.ToString("yyyy-MM-dd"), this.Nzmca,
            this.Message, this.Subject, DateTime.Now.ToString("g"));
            return sRtn;
        }
    }
    public string ConfirmBkgTxtMsg
    {
        get
        {
            string sRtn = string.Format("Hi {0} ," +
                "\r\n" +
            "Booking for: {5} \r\n" +
            "        Arrival {1} after mid-day and departing {2} before mid-day.\r\n" +
            "Address is\r\n" +
            "  22 Fitzpatrick St\r\n" +
            "  Newlands\r\n" +
            "  Wellington 6037\r\n" +
            "Access is best from Stewart Drive.Either from the north where you would exit SH1 at Johnsonville or from the South turning into Newlands. Turn into Fitzpatrick St from Stewart Drive is best as other routes(courtesy of the map apps) send you around a very narrow road.We are on the left corner of the 2nd street on the left going up Fitzpatrick St.\r\n" +
            "Payment is $30 for the night with NZMCA Memebrship {3}, payable in advance please to our bank account.\r\n" +
            "        Ronald Pontifex\r\n" +
            "        03 0525 0253043 000\r\n" +
            "(We have Wise Card if that suits you better.)\r\n\r\n" +
            "Please txt on arrival\r\n" +
            "Chrs\r\n" +
            "Ron Pontifex\r\n" +
            "WELLINGTON NZ\r\n" +
            "Ph 027 304 2002\r\n" +
            "Email: rvpark@pontifex.nz\r\n" +
             "Date: {6}",
            this.FullName, this.Fmdate.ToString("yyyy-MM-dd"), this.Todate.ToString("yyyy-MM-dd"), this.Nzmca,
            this.Message, this.Subject, DateTime.Now.ToString("g"));
            return sRtn;
        }
    }
    //Not ready yet
    public string ConfirmMsg
    {
        get
        {
            string sRtn = string.Format("Hi {0} \r\n" +
                "\r\n \r\n" +
                "Date: {8}\r\n\r\n" +
            "Thanks for your interest in RV Park on Fitz.\r\n\r\nThis email confirms your booking with the details as you will see below .\r\n \r\n" +
            "Booking for: [{6}] \r\n\r\n" +
            "Your booking is confirmed:\r\n" +
            "        Arrival {3} after mid-day and departing {4} before mid-day.\r\n\r\n" +
            "Address is\r\n" +
            "        22 Fitzpatrick St\r\n" +
            "        Newlands\r\n" +
            "        Wellington 6037\r\n\r\n" +
            "Other Details we have for your are:-\r\n" +
            "        Your email address is [{1}] \r\n" +
            "        Your Phone number is [{2}] \r\n" +
            "        If you are an NZMCA member your number being [{5}] \r\n" +


            "Access is best from Stewart Drive.Either from the north where you would exit SH1 at Johnsonville or from the South turning into Newlands. Turn into Fitzpatrick St from Stewart Drive is best as other routes(courtesy of the map apps) send you around a very narrow road.We are on the left corner of the 2nd street on the left going up Fitzpatrick St.\r\n" +
            "Access from Stewart Drive is also possible via Quigley St which is at the crest of the hill. Quigley St flows around until it comes to a T-intersection with Fitzpatrick St and we are on that corner.\r\n\r\n" +

            "Payment is $30 for the night (for NZMCA members), payable in advance please to our bank account.\r\n" +
            "        Ronald Pontifex\r\n" +
            "        03 0525 0253043 000\r\n\r\n" +
            "(We have Wise Card if that suits you better.)\r\n\r\n" +
            "Please text us on arrival and we shall come out to meets you etc.\r\n\r\n" +
            "Again many thanks\r\n\r\n" +
            "Ron Pontifex\r\n" +
            "WELLINGTON New Zealand\r\n" +
            "Ph 027 304 2002\r\n" +
            "E - Mail : rvpark@pontifex.nz\r\n",
            this.Name, this.Email, this.Phone,
            this.Fmdate.ToString("yyyy-MM-dd"), this.Todate.ToString("yyyy-MM-dd"), this.Nzmca,
            this.Message, this.Subject, DateTime.Now.ToString("g"));
            return sRtn;
        }
    }

    public bool IsValidEmail(string Email)
    {
        bool rtn = false;
        if (Email == string.Empty || Email == null || Email == "") return false;
        try
        {
            MailAddress m = new MailAddress(Email);
            rtn = true;
        }
        catch (FormatException)
        {
            rtn  = false;
        }
        if (rtn)
        {
            rtn = HasValidMxRecord(Email);
        }
        return rtn;
    }
    public bool HasValidMxRecord(string email)
    {
        try
        {
            string domain = email.Split('@')[1];
            IPHostEntry host = Dns.GetHostEntry(domain);
            return host.AddressList.Any(ip => ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6);
        }
        catch
        {
            return false;
        }
    }

}