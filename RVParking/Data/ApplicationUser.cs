using Microsoft.AspNetCore.Identity;

namespace RVParking.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string FirstNames { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }

}
