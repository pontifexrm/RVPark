# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RVPark is an RV Park booking and management system built with ASP.NET Core 10.0 Blazor (Interactive Server-Side Rendering). It manages RV park properties, bookings, availability calendars, and user accounts.

## Build & Run Commands

```bash
# Build
dotnet build
dotnet build -c Release

# Run locally (HTTP: localhost:5000, HTTPS: localhost:5001)
dotnet run

# Database migrations
dotnet ef database update

# Publish profiles available in Properties/PublishProfiles/
```

## Technology Stack

- **Framework:** ASP.NET Core 10.0, Blazor Server
- **Database:** SQL Server with Entity Framework Core 10.0.1
- **Auth:** ASP.NET Core Identity with roles (Admin, Manager, User)
- **UI Components:** Syncfusion Blazor 32.x, Blazor.Bootstrap 3.5, Bootstrap 5.3
- **Email:** MailKit (primary), SMTP, TNZ API (pluggable via DI)
- **SMS:** SMSEveryone, TNZ SMS (pluggable via DI)
- **Mapping:** AutoMapper 16.0
- **CQRS/Mediator:** MediatR 12.4.0
- **Validation:** FluentValidation 11.11.0

## Architecture

The project uses a hybrid architecture combining traditional Blazor structure with Vertical Slice Architecture (VSA) for core features.

```
RVPark/
├── Components/           # Razor components (UI)
│   ├── Account/          # Identity/auth components
│   ├── Admin/            # Admin management pages
│   ├── Layout/           # MainLayout, NavMenu, NavBar
│   └── Pages/            # Public-facing pages
├── Data/                 # Entity models & DbContext
│   ├── ApplicationDbContext.cs
│   ├── Bkg_*.cs          # Booking domain models
│   └── Bkg_Engine.cs     # Legacy booking logic (kept for reference)
├── Features/             # Vertical Slice Architecture (VSA)
│   ├── Authentication/   # Login, Register, Logout
│   │   ├── Commands/     # LoginCommand, RegisterCommand, LogoutCommand
│   │   ├── Queries/      # GetCurrentUserQuery
│   │   ├── DTOs/         # AuthResultDto, CurrentUserDto
│   │   ├── Services/     # IAuthenticationService, AuthenticationService
│   │   └── Pages/        # Login.razor, Register.razor
│   ├── Bookings/         # Booking management
│   │   ├── Commands/     # CreateBooking, EditBooking, DeleteBooking
│   │   ├── Queries/      # GetBookingById, GetUserBookings, CheckAvailability
│   │   ├── DTOs/         # BookingDto
│   │   ├── Services/     # IBookingEngine, BookingEngine (7 edit scenarios)
│   │   └── MappingProfiles/
│   └── Properties/       # Property management
│       ├── Commands/     # UpdatePropertyCommand
│       ├── Queries/      # GetAllProperties, GetPropertyById
│       ├── DTOs/         # PropertyDto
│       ├── Pages/        # PropertyList.razor
│       └── MappingProfiles/
├── Shared/               # Cross-cutting concerns
│   ├── Result.cs         # Result<T> pattern for operation outcomes
│   └── Behaviors/        # MediatR pipeline behaviors
│       └── ValidationBehavior.cs
├── Services/             # Infrastructure services
│   ├── Email/            # IEmailService implementations
│   ├── SMS/              # ISmsService implementations
│   ├── Logging/          # IAppLogger, request middleware
│   └── ServiceExtensions.cs
├── Migrations/           # EF Core migrations
└── Program.cs            # Startup configuration & DI registration
```

## Core Domain Models (Data/)

- **Bkg_Property** - RV park property with amenities
- **Bkg_Booking** - Booking record (DateArrive, DateDepart, TotalPrice, Status)
- **Bkg_Availability** - Daily availability calendar per property
- **Bkg_User** - Guest profile (separate from Identity user)
- **Bkg_Payment** - Payment tracking
- **Bkg_Review** - Guest reviews/ratings

## BookingEngine Service

The `IBookingEngine` (in `Features/Bookings/Services/`) handles all booking operations with transaction safety:

- `RequestBooking()` - Check availability for requested dates
- `CreateBookingAsync()` - Create new booking with availability updates
- `EditBookingAsync()` - Handle 7 edit scenarios (A-G) for date/property changes
- `DeleteBookingAsync()` - Remove booking and restore availability
- `IsAvailable()` - Check date range availability

**Edit Scenarios (A-G):**
```
                    A1----------D1
(a) A2----------D2   |           |     New dates entirely before current
(b)         A2----------D2       |     New arrival before, depart within
(c)      A2---------------------------D2   New spans over current
(d)                  | A2----D2  |     New entirely within current
(e)                  |      A2----------D2  New arrival within, depart after
(f)                  |           |   A2----------D2  New dates entirely after
(g)  P1 != P2  Property change - treat as delete + create
```

## Key Pages

- **Availability.razor** - Property availability search
- **Booking.razor** - Booking workflow
- **MyBookings.razor** - User's bookings view (uses IBookingEngine)
- **AllBookings.razor** - Admin booking management (uses IBookingEngine)
- **ViewBkgCalendar.razor** - Calendar-based booking view
- **ContactRegister.razor** - Registration and contact form (uses IBookingEngine)

## Services Pattern

### Booking Engine (VSA)
Injected via DI: `@inject IBookingEngine BookingEngine`

Registered in Program.cs:
```csharp
builder.Services.AddScoped<IBookingEngine, BookingEngine>();
```

### MediatR Commands/Queries
```csharp
// Send a command
var result = await Mediator.Send(new CreateBookingCommand(...));

// Send a query
var bookings = await Mediator.Send(new GetUserBookingsQuery(userId));
```

### Email and SMS
Pluggable implementations configured in `appsettings.json`:
- Email: `IEmailService` → MailKitEmailService, SmtpEmailService, TNZEmailService, MockEmailService
- SMS: `ISmsService` → SmsEveryoneService, SmsTNZService, SmsMockService

## Configuration

- `appsettings.json` - Main config (connection strings, email/SMS settings)
- `appsettings.Development.json` / `appsettings.Production.json` - Environment overrides
- Localization: en-NZ (New Zealand English)
- Authentication: 5-day session expiration, 3-hour token lifespan

## Database

- Local dev: `RONSHPLAPTOP\SQLEXPRESS`, database `db_ac1621_rvpark`
- Migrations managed via dotnet-ef tool
- DbContext includes helper methods for booking/availability queries

## DI Registration (Program.cs)

Key service registrations:
```csharp
// MediatR with validation pipeline
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Booking Engine
builder.Services.AddScoped<IBookingEngine, BookingEngine>();

// Authentication Service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
```

## VSA Conversion Status Report
● VSA Conversion Status Report as at 06/02/2026 after a fair bit of work converting core booking features to Vertical Slice Architecture (VSA). This report summarizes the current state of the codebase, highlighting what has been converted, what remains in legacy structure, and recommendations for next steps.

  ### Summary

  The project is partially converted to VSA. Core booking functionality uses MediatR, but significant legacy code remains.

  ---
  Features Folder (VSA Structure)
  ┌────────────────┬─────────────────────────────┬───────────────────────────────────────────────────────────────────────┬────────────┬──────┬─────────┬─────────────┐
  │    Feature     │          Commands           │                                Queries                                │ Validators │ DTOs │ Mapping │    Pages    │
  ├────────────────┼─────────────────────────────┼───────────────────────────────────────────────────────────────────────┼────────────┼──────┼─────────┼─────────────┤
  │ Authentication │ 3 (Login, Logout, Register) │ 1 (GetCurrentUser)                                                    │ 2          │ 2    │ -       │ 2 + 7 Admin │
  ├────────────────┼─────────────────────────────┼───────────────────────────────────────────────────────────────────────┼────────────┼──────┼─────────┼─────────────┤
  │ Bookings       │ 3 (Create, Edit, Delete)    │ 4 (GetById, GetUserBookings, CheckAvailability, GetBkgUserByUsername) │ 3          │ 2    │ 1       │ 9           │
  ├────────────────┼─────────────────────────────┼───────────────────────────────────────────────────────────────────────┼────────────┼──────┼─────────┼─────────────┤
  │ Properties     │ 1 (UpdateProperty)          │ 2 (GetAll, GetById)                                                   │ 1          │ 1    │ 1       │ 6           │
  ├────────────────┼─────────────────────────────┼───────────────────────────────────────────────────────────────────────┼────────────┼──────┼─────────┼─────────────┤
  │ Admin          │ -                           │ -                                                                     │ -          │ -    │ -       │ 3           │
  └────────────────┴─────────────────────────────┴───────────────────────────────────────────────────────────────────────┴────────────┴──────┴─────────┴─────────────┘
  Total in Features/: 46 .cs files, 36 .razor pages

  ---
  Legacy Code Remaining (NOT in VSA)

  Components/Pages/ (18 pages)

  - Home.razor, About.razor, Error.razor, Auth.razor
  - ContactUs.razor, Thingstoknow.razor, Thingstoknowdetail.razor
  - TabComponents/ (11 maintenance pages):
    - DB_Maint.razor, DBTblNavMenu.razor, DynamicCRUD.razor
    - AspNetRoles_Maint.razor, AspNetUser_Maint.razor, AspNetUserRoles_Maint.razor
    - Bkg_Bookings_Maint.razor, Bkg_User_Maint.razor, Bkg_Property_Maint.razor
    - Bkg_Availability_Maint.razor, BkgUser_Bookings.razor

  Components/Account/ (40+ pages)

  - All Identity scaffolded pages remain in legacy location
  - Login, Register, ForgotPassword, 2FA, Manage/* etc.

  Data/ Folder (Legacy Services & Models)

  - Bkg_Engine.cs - Old booking engine (kept for reference)
  - Bkg_UserService.cs - Should move to Features/Bookings/Services/
  - EmailSenderMailKit.cs, EmailSenderSystem.cs - Duplicates of Services/Email/
  - SmsService.cs, SmsModels.cs - Duplicates of Services/SMS/
  - MappingProfile.cs - Legacy mapping (should consolidate)
  - Various helper classes that could move to Shared/

  ---
  MediatR Usage
  ┌───────────────────┬──────────────────────────────────────────────────────────┐
  │      Status       │                       Description                        │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Implemented       │ All booking CRUD operations use MediatR commands/queries │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Implemented       │ CheckAvailability query returns Result<bool>             │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Implemented       │ BookingEngine returns Result<T> with error messages      │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Implemented       │ ValidationBehavior pipeline in place                     │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Not Using MediatR │ TabComponent maintenance pages (direct DbContext)        │
  ├───────────────────┼──────────────────────────────────────────────────────────┤
  │ Not Using MediatR │ Identity/Account pages                                   │
  └───────────────────┴──────────────────────────────────────────────────────────┘
  ---
  FluentValidation

  6 validators exist:
  - LoginValidator, RegisterValidator
  - CreateBookingValidator, EditBookingValidator, DeleteBookingValidator
  - UpdatePropertyValidator

  Missing validators for: Queries, other commands

  ---
  AutoMapper

  2 mapping profiles:
  - BookingMappingProfile (Bkg_Booking → BookingDto)
  - PropertyMappingProfile (Bkg_Property → PropertyDto)

  Legacy: Data/MappingProfile.cs still exists

  ---
  Recommended Next Steps

  1. High Priority:
    - Move Components/Pages/ContactUs.razor to Features/
    - Create MediatR queries for TabComponent pages or migrate them
  2. Medium Priority:
    - Consolidate duplicate services in Data/ folder
    - Move Bkg_UserService.cs to Features/Bookings/Services/
    - Delete Data/Bkg_Engine.cs (legacy, replaced by BookingEngine)
  3. Lower Priority:
    - Migrate Identity/Account pages to Features/Authentication/
    - Add missing validators for queries
    - Create DTOs for remaining entities (Bkg_User, Bkg_Availability, etc.)

  ---
  ### Completion Estimate as at 06/02/2026
  ┌────────────────────────┬─────────────────────────────────────────────┐
  │          Area          │                   Status                    │
  ├────────────────────────┼─────────────────────────────────────────────┤
  │ Bookings Feature       │ ~90% complete                               │
  ├────────────────────────┼─────────────────────────────────────────────┤
  │ Properties Feature     │ ~70% complete                               │
  ├────────────────────────┼─────────────────────────────────────────────┤
  │ Authentication Feature │ ~50% complete (Identity pages not migrated) │
  ├────────────────────────┼─────────────────────────────────────────────┤
  │ Admin Feature          │ ~30% complete                               │
  ├────────────────────────┼─────────────────────────────────────────────┤
  │ Overall VSA Conversion │ ~55%                                        │
  └────────────────────────┴─────────────────────────────────────────────┘
