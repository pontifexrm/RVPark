
using System.ComponentModel.DataAnnotations;

namespace RVParking.Data
{
    public class Contact
    {
        public int Id { get; set; }
        [Required]
        [MinLength(5)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Body { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
    }
}
