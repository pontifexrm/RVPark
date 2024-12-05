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
   
    public string Message { get; set; } = string.Empty;
    
    public string Nzmca { get; set; } = string.Empty;

    public DateTime Fmdate { get; set; } = DateTime.Now.Date;

    public DateTime Todate { get; set; } = DateTime.Now.Date;


    public string ChkMsg
    { 
        get{
            string sRtn = string.Format("Hi {0} \r\n" +
                              "Thanks for your interest in RV Park NZ.\r\n\r\nWe have received your message and will be contacting you soon with out response. \r\n" +
                              "Just so you have a record of this message here are the details you sent to us. \r\n\r\n" +
                              "SUBJECT: [{7}]\r\n" +
                              "Your email address is [{1}] \r\n" +
                              "Your Phone number is [{2}] \r\n" +
                              "You are interested in dates from [{3}] to [{4}] \r\n" +
                              "If you are an NZMCA member your number being [{5}] \r\n" +
                              "And finally your message is [{6}] \r\n\r\n" +
                              "Many thanks \r\n" +
                              "RV Park on Fitz \r\n",
            this.Name, this.Email, this.Phone,
            this.Fmdate, this.Todate, this.Nzmca,
            this.Message, this.Subject);
            return sRtn;
        } 
    }
}