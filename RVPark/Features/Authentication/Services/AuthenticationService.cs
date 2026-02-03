using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Authentication.DTOs;

namespace RVPark.Features.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResultDto> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return new AuthResultDto(false, ErrorMessage: "Invalid login attempt.");
        }

        // Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            return new AuthResultDto(false, RequiresEmailConfirmation: true,
                ErrorMessage: "You must confirm your email before logging in.");
        }

        // Validate password
        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            // Log failed attempt
            await LogLoginAttemptAsync(null, false);
            return new AuthResultDto(false, ErrorMessage: "Invalid login attempt.");
        }

        // Check for lockout
        if (await _userManager.IsLockedOutAsync(user))
        {
            return new AuthResultDto(false, IsLockedOut: true,
                ErrorMessage: "Account is locked out.");
        }

        // Check for 2FA
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return new AuthResultDto(false, RequiresTwoFactor: true, UserId: user.Id);
        }

        // Log successful attempt
        await LogLoginAttemptAsync(user.Id, true);

        return new AuthResultDto(
            Succeeded: true,
            UserId: user.Id,
            Email: user.Email,
            FirstName: user.FirstNames,
            LastName: user.LastName
        );
    }

    public async Task<AuthResultDto> RegisterAsync(string email, string password, string firstName, string lastName, string? mobile)
    {
        // Check if this is the first user (auto-create Admin role)
        var userCount = await _dbContext.GetIdentityUserCountAsync();
        if (userCount == 0)
        {
            await EnsureAdminRoleExistsAsync();
        }

        // Create the Identity user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstNames = firstName,
            LastName = lastName,
            PhoneNumber = mobile,
            CreatedDte = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return new AuthResultDto(
                Succeeded: false,
                Errors: result.Errors.Select(e => e.Description),
                ErrorMessage: string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // Create or update Bkg_User
        await CreateOrUpdateBkgUserAsync(user, firstName, lastName, mobile);

        // If first user, add to Admin role
        if (userCount == 0)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return new AuthResultDto(
            Succeeded: true,
            UserId: user.Id,
            Email: user.Email,
            FirstName: user.FirstNames,
            LastName: user.LastName,
            RequiresEmailConfirmation: _userManager.Options.SignIn.RequireConfirmedAccount
        );
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await _signInManager.SignOutAsync();
        }
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var claimsPrincipal = authState.User;

        if (claimsPrincipal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var bkgUser = await _dbContext.bkg_Users!
            .FirstOrDefaultAsync(u => u.AppUserId == user.Id);

        return new CurrentUserDto(
            UserId: user.Id,
            Email: user.Email ?? string.Empty,
            UserName: user.UserName ?? string.Empty,
            FirstName: user.FirstNames,
            LastName: user.LastName,
            PhoneNumber: user.PhoneNumber,
            EmailConfirmed: user.EmailConfirmed,
            Roles: roles,
            BkgUserId: bkgUser?.UserId
        );
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User?.Identity?.IsAuthenticated == true;
    }

    #region Private Helpers

    private async Task LogLoginAttemptAsync(string? userId, bool success)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var loginLog = new LoginLog
        {
            UserId = userId,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            Success = success,
            Timestamp = DateTimeOffset.UtcNow
        };

        _dbContext.LoginLogs!.Add(loginLog);
        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureAdminRoleExistsAsync()
    {
        var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            var role = new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            };
            await _dbContext.Roles.AddAsync(role);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task CreateOrUpdateBkgUserAsync(ApplicationUser user, string firstName, string lastName, string? mobile)
    {
        var existingBkgUser = await _dbContext.bkg_Users!
            .FirstOrDefaultAsync(u => u.UserEmail == user.Email);

        if (existingBkgUser != null)
        {
            // Update existing Bkg_User
            existingBkgUser.AppUserId = user.Id;
            existingBkgUser.UserName = user.Email ?? string.Empty;

            // Also update ApplicationUser with Bkg_User data
            user.FirstNames = existingBkgUser.UserFirstName;
            user.LastName = existingBkgUser.UserLastName;
            user.NZMCA = existingBkgUser.UserNZMCA;
            user.City = existingBkgUser.UserCity;
            user.Country = existingBkgUser.UserCountry;
            user.UserAddress = existingBkgUser.UserAddress;
            user.UserState = existingBkgUser.UserState;
            user.UserZip = existingBkgUser.UserZip;

            _dbContext.bkg_Users!.Update(existingBkgUser);
            _dbContext.Users.Update(user);
        }
        else
        {
            // Create new Bkg_User
            var bkgUser = new Bkg_User
            {
                UserEmail = user.Email ?? string.Empty,
                UserName = user.Email ?? string.Empty,
                AppUserId = user.Id,
                UserFirstName = firstName,
                UserLastName = lastName,
                UserPhone = mobile ?? string.Empty,
                CreatedDte = DateTime.UtcNow
            };
            await _dbContext.bkg_Users!.AddAsync(bkgUser);
        }

        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
