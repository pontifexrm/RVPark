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

## Architecture

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
│   └── Bkg_Engine.cs     # Booking business logic (618 lines)
├── Services/             # Business logic & integrations
│   ├── Email/            # IEmailService implementations
│   ├── SMS/              # ISmsService implementations
│   ├── Logging/          # IAppLogger, request middleware
│   └── ServiceExtensions.cs
├── Migrations/           # EF Core migrations
└── Program.cs            # Startup configuration
```

## Core Domain Models (Data/)

- **Bkg_Property** - RV park property with amenities
- **Bkg_Booking** - Booking record (DateArrive, DateDepart, TotalPrice, Status)
- **Bkg_Availability** - Daily availability calendar per property
- **Bkg_User** - Guest profile (separate from Identity user)
- **Bkg_Payment** - Payment tracking
- **Bkg_Review** - Guest reviews/ratings
- **Bkg_Engine** - Central booking business logic

## Key Pages

- **Availability.razor** - Property availability search
- **Booking.razor** - Booking workflow
- **MyBookings.razor** - User's bookings view
- **AllBookings.razor** - Admin booking management
- **ViewBkgCalendar.razor** - Calendar-based booking view
- **ContactRegister.razor** - Registration and contact form

## Services Pattern

Email and SMS services use pluggable implementations configured in `appsettings.json`:
- Email: `IEmailService` → MailKitEmailService, SmtpEmailService, TNZEmailService, MockEmailService
- SMS: `ISmsService` → SmsEveryoneService, SmsTNZService, SmsMockService

Services registered in `Program.cs` and `Services/ServiceExtensions.cs`.

## Configuration

- `appsettings.json` - Main config (connection strings, email/SMS settings)
- `appsettings.Development.json` / `appsettings.Production.json` - Environment overrides
- Localization: en-NZ (New Zealand English)
- Authentication: 5-day session expiration, 3-hour token lifespan

## Database

- Local dev: `RONSHPLAPTOP\SQLEXPRESS`, database `db_ac1621_rvpark`
- Migrations managed via dotnet-ef tool
- DbContext includes helper methods for booking/availability queries
