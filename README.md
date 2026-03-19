<div align="center">

# 🎮 Station Pro

### A powerful SaaS platform for managing PlayStation & gaming stations

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
[![Hangfire](https://img.shields.io/badge/Hangfire-Background_Jobs-FF6600?style=for-the-badge)](https://www.hangfire.io/)

> Station Pro helps gaming shop owners track devices, rooms, sessions, and subscription plans — all from one clean multi-tenant dashboard.

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Authentication & Multi-Tenancy](#-authentication--multi-tenancy)
- [Background Jobs (Hangfire)](#-background-jobs-hangfire)
- [Subscription System](#-subscription-system)
- [Database Design](#-database-design)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)

---

## 🌟 Overview

**Station Pro** is a multi-tenant SaaS application built for gaming stores and PlayStation lounges. Each shop owner (tenant) gets their own isolated workspace to manage devices, rooms, gaming sessions, and billing — all behind a secure subscription model.

The platform supports a full admin panel for managing tenants, reviewing subscription requests, approving/rejecting payments, and overriding plans.

---

## ✨ Features

### 🏪 For Shop Owners (Tenants)
| Feature | Description |
|---|---|
| 🖥️ **Device Management** | Add and manage PS5, PS4, Xbox, PC, and any custom device types |
| 🏠 **Room Management** | Manage VIP rooms and regular rooms with reservation support |
| ⏱️ **Session Tracking** | Start/end sessions per device or room, track usage time in real-time |
| 💰 **Auto Cost Calculation** | Session cost is auto-calculated based on hourly rate × duration |
| 📊 **Live Dashboard** | Real-time stats: active sessions, today's revenue, device cards |
| 📅 **Session History** | Filter and search sessions by date, device, status, and more |
| 🔐 **Forgot / Reset Password** | Secure email-based password reset flow with expiring tokens |
| 🌍 **Multi-language** | Supports English (`en-US`) and Arabic (`ar-EG`) with RTL layout |

### 🛡️ For Admins
| Feature | Description |
|---|---|
| 👥 **Tenant Management** | View all tenants, toggle active/inactive status |
| 📋 **Subscription Requests** | Review payment proofs and approve or reject subscription requests |
| 🏷️ **Plan Override** | Manually assign or override a tenant's subscription plan |
| 📈 **Dashboard Stats** | System-wide statistics across all tenants |

---

## 🏗️ Architecture

Station Pro is built on **Clean Architecture** (also known as Onion Architecture), strictly separating concerns across 4 layers:

```
┌─────────────────────────────────────────────────┐
│              StationPro (Presentation)           │
│     MVC Controllers · Views · Filters ·          │
│     Middleware · Program.cs · wwwroot            │
├─────────────────────────────────────────────────┤
│            StationPro.Application                │
│     Interfaces · DTOs · Service Contracts ·      │
│     Use Case Definitions                         │
├─────────────────────────────────────────────────┤
│              StationPro.Domain                   │
│     Entities · Enums · Domain Rules ·            │
│     Base Classes · ITenantEntity                 │
├─────────────────────────────────────────────────┤
│           StationPro.Infrastructure              │
│     EF Core · Repositories · Email Service ·     │
│     DbContext · Migrations · DI Registration     │
└─────────────────────────────────────────────────┘
```

**Dependency rule:** Each layer only depends on the layer directly below it. The Domain layer has zero external dependencies.

---

## 🛠️ Tech Stack

### Backend
| Technology | Version | Purpose |
|---|---|---|
| **ASP.NET Core MVC** | .NET 8 | Web framework, controllers, views |
| **Entity Framework Core** | 8.x | ORM, database migrations, query filters |
| **SQL Server** | Latest | Primary relational database |
| **Hangfire** | Latest | Background job processing & scheduling |
| **Hangfire.SqlServer** | Latest | Hangfire persistence storage |
| **BCrypt.Net** | Latest | Password hashing |
| **ASP.NET Cookie Auth** | .NET 8 | Authentication via encrypted cookies |
| **Data Protection API** | .NET 8 | Key persistence across app restarts |

### Frontend
| Technology | Purpose |
|---|---|
| **Razor Views (.cshtml)** | Server-side HTML rendering |
| **HTMX** | Partial page updates without full page reload |
| **Tailwind CSS** | Utility-first CSS styling |
| **JavaScript (Vanilla)** | Client-side interactivity |

### Infrastructure
| Technology | Purpose |
|---|---|
| **SMTP (Gmail)** | Transactional email (password reset, notifications) |
| **Response Compression** | Brotli + Gzip for faster delivery |
| **ASP.NET Localization** | Cookie + query string based culture switching |

---

## 📁 Project Structure

```
Solution 'StationPro' (4 projects)
│
├── StationPro/                          ← Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs            ← Login, Register, Password Reset
│   │   ├── AdminController.cs           ← Admin panel (tenant + subscription mgmt)
│   │   ├── DashboardController.cs       ← Live stats & session management
│   │   ├── DeviceController.cs          ← Device CRUD + session start/end
│   │   ├── RoomController.cs            ← Room CRUD + reservations + sessions
│   │   ├── SessionController.cs         ← Unified session history & details
│   │   ├── SubscriptionController.cs    ← Subscribe, Pending, Rejected flows
│   │   └── LanguageController.cs        ← Culture switcher
│   ├── Filters/
│   │   ├── AdminAuthFilter.cs           ← Blocks non-admin access to /Admin/*
│   │   ├── SubscriptionGuardFilter.cs   ← Blocks access without active subscription
│   │   └── HangfireBasicAuthFilter.cs   ← HTTP Basic Auth for Hangfire dashboard
│   ├── Middlewares/
│   │   ├── TenantResolutionMiddleware.cs ← Reads TenantId from cookie claims
│   │   └── TenantGuardMiddleware.cs      ← Blocks protected routes with no tenant
│   ├── Views/                           ← Razor views per controller
│   ├── wwwroot/                         ← Static assets (CSS, JS, images)
│   └── Program.cs                       ← App bootstrap & DI registration
│
├── StationPro.Application/              ← Application Layer
│   ├── Contracts/
│   │   ├── Repositories/                ← Repository interfaces
│   │   └── Services/                    ← Service interfaces
│   ├── DTOs/                            ← Data Transfer Objects
│   ├── Interfaces/                      ← Additional service contracts
│   └── Services/                        ← Service implementations (non-infra)
│
├── StationPro.Domain/                   ← Domain Layer
│   ├── Entities/
│   │   ├── Tenant.cs
│   │   ├── Device.cs
│   │   ├── Room.cs
│   │   ├── Session.cs
│   │   ├── SubscriptionRequest.cs
│   │   └── Admin.cs
│   ├── Enums/                           ← SubscriptionPlan, SessionStatus, etc.
│   └── Common/
│       ├── BaseEntity.cs                ← Id, CreatedAt, UpdatedAt
│       └── ITenantEntity.cs             ← TenantId contract for query filters
│
└── StationPro.Infrastructure/           ← Infrastructure Layer
    ├── Data/
    │   ├── ApplicationDbContext.cs      ← EF Core context with global query filters
    │   └── Configurations/              ← Fluent API entity configs
    ├── Repositories/                    ← EF Core repository implementations
    ├── Services/
    │   ├── AuthService.cs               ← Register, Login, Password Reset
    │   ├── EmailService.cs              ← SMTP email sending
    │   └── TenantService.cs             ← Resolves current TenantId
    └── DependencyInjection.cs           ← AddInfrastructureService() extension
```

---

## 🔐 Authentication & Multi-Tenancy

### Authentication Flow

Station Pro uses **ASP.NET Core Cookie Authentication** — no JWT tokens. Upon login, a signed and encrypted cookie is issued containing the tenant's claims.

```
User logs in
    ↓
AuthController validates credentials via BCrypt
    ↓
Issues cookie with claims: TenantId, Role
    ↓
TenantResolutionMiddleware reads TenantId from claims
    ↓
TenantGuardMiddleware blocks protected routes if no TenantId
    ↓
ApplicationDbContext global query filters scope all queries to that TenantId
```

### Global Query Filters (Data Isolation)

Every tenant only sees their own data. This is enforced automatically at the EF Core level:

```csharp
modelBuilder.Entity<Device>()
    .HasQueryFilter(d => d.TenantId == _tenantService.TryGetCurrentTenantId() ?? 0);
```

This filter applies to every query automatically — no manual `WHERE TenantId = X` needed anywhere in the codebase.

### Middleware Pipeline Order

The order of middleware in `Program.cs` is critical:

```
UseAuthentication()              → Decrypts cookie → populates HttpContext.User
TenantResolutionMiddleware       → Reads TenantId from claims → HttpContext.Items
TenantGuardMiddleware            → Blocks if no TenantId on protected routes
UseAuthorization()               → Standard ASP.NET authorization
UseHangfireDashboard()           → Protected by Basic Auth filter
```

---

## ⚙️ Background Jobs (Hangfire)

Station Pro uses **Hangfire** to handle background jobs reliably — primarily for sending emails without blocking the HTTP request.

### Why Hangfire?

Without Hangfire, if the SMTP server is temporarily down, the email is lost forever. With Hangfire, failed jobs are automatically retried with exponential backoff.

### Setup

Hangfire uses a **dedicated, isolated database** (`db44413_hangfire`) separate from the main application database. This keeps Hangfire's internal polling tables from interfering with business data, migrations, or backups.

```
Main DB (db44413)          → All business entities (Tenants, Devices, Sessions...)
Hangfire DB (db44413_hangfire) → Hangfire job tables only
```

### Enqueuing Email Jobs

```csharp
// Enqueue a background job — fire and forget with automatic retry
BackgroundJob.Enqueue<IEmailService>(e =>
    e.SendAsync(tenant.Email, "Reset your StationPro password", resetEmailBody));
```

### Dashboard

The Hangfire dashboard is available at `/hangfire` and secured with HTTP Basic Authentication (username/password stored in `appsettings.json`). It shows all jobs, their status, retry history, and execution times.

---

## 💳 Subscription System

Station Pro has a built-in subscription approval workflow:

```
Tenant registers
    ↓
Chooses a plan (Basic / Pro / Enterprise)
    ↓
Uploads payment proof screenshot + enters transaction reference
    ↓
Request goes to "Pending" state
    ↓
Admin reviews payment proof in admin panel
    ↓
Admin Approves → Tenant is activated, plan is assigned
Admin Rejects  → Tenant sees rejection reason, can resubmit
```

### Subscription Plans

| Plan | Target |
|---|---|
| **Free** | Default placeholder before admin assigns real plan |
| **Basic** | Small shops |
| **Pro** | Medium shops |
| **Enterprise** | Large multi-device stores |

Each plan can have limits on devices, sessions, and features (enforced via `SubscriptionGuardFilter`).

---

## 🗄️ Database Design

### Core Entities

```
Tenant
  ├── Devices (1→N)
  ├── Rooms (1→N)
  ├── Sessions (1→N)
  └── SubscriptionRequests (1→N)

Session
  ├── DeviceId (nullable)   ← device session
  └── RoomId (nullable)     ← room session

Admin (separate — not a Tenant)
```

### Key Design Decisions

- **`ITenantEntity`** — interface implemented by `Device`, `Room`, `Session`. Required for the global query filter to work generically.
- **`BaseEntity`** — base class with `Id`, `CreatedAt`, `UpdatedAt`. All entities inherit from it.
- **`SubscriptionPlan` enum** stored on `Tenant` — set by admin after payment approval.
- **`IsActive` flag on Tenant** — admin can deactivate a tenant at any time, immediately locking them out.
- **Password reset tokens** stored on `Tenant` with a 1-hour expiry.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (local or remote)
- A Gmail account with App Password for SMTP

### Clone & Setup

```bash
git clone https://github.com/your-username/StationPro.git
cd StationPro
```

### Database Setup

```bash
# Apply EF Core migrations
dotnet ef database update --project StationPro.Infrastructure --startup-project StationPro
```

Hangfire will auto-create its own tables on first startup — no migrations needed for the Hangfire database.

### Run

```bash
dotnet run --project StationPro
```

---

## ⚙️ Configuration

Update `appsettings.json` with your own values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=StationPro;...",
    "HangfireConnection": "Server=...;Database=StationPro_Hangfire;..."
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromName": "StationPro",
    "FromAddress": "your-email@gmail.com"
  },
  "Hangfire": {
    "DashboardUsername": "admin",
    "DashboardPassword": "YourStrongPassword123!"
  }
}
```

> ⚠️ **Never commit real credentials to source control.** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in development and environment variables in production.

---

<div align="center">

Built with ❤️ using ASP.NET Core 8 · Clean Architecture · Multi-Tenancy

</div>
