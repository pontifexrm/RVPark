using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace RVParking.Data;

public class EmailMessage
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Phone { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty; 
    [Required]
    public string Subject { get; set; } = string.Empty;
    [Required]
    public string Message { get; set; } = string.Empty;
    [Required]
    public string Nzmca { get; set; } = string.Empty;

    public DateTime Fmdate { get; set; } = DateTime.Now.Date;

    public DateTime Todate { get; set; } = DateTime.Now.Date;

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
                              "{5}",
            this.Name, this.Email, this.E_164_Phone,
            this.Subject, this.Message, DateTime.Now.ToString("g"));
            return sRtn;
        }
    }
}