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
        public string NZMCA { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;
        public string UserState { get; set; } = string.Empty;
        public string UserZip { get; set; } = string.Empty;
        public string UserStatus { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;

    }

}
