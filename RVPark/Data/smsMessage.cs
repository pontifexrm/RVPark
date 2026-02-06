using System.ComponentModel.DataAnnotations;

namespace RVPark.Data;

public class smsMessage
{

    //public string Email { get; set; } = string.Empty;

    //public string Phone { get; set; } = string.Empty;

    //public string Name { get; set; } = string.Empty; 

    //public string Subject { get; set; } = string.Empty;
    [Required]
    public string Message { get; set; } = string.Empty;




}
