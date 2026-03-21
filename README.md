<div align="center">

# 🎮 Station Pro

### A powerful SaaS platform for managing PlayStation & gaming stations

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
[![Hangfire](https://img.shields.io/badge/Hangfire-Background_Jobs-FF6600?style=for-the-badge)](https://www.hangfire.io/)
[![Live Demo](https://img.shields.io/badge/🌐_Live_Demo-stationpro.runasp.net-22C55E?style=for-the-badge)](https://stationpro.runasp.net/)

> Station Pro helps gaming shop owners track devices, rooms, sessions, and subscription plans — all from one clean multi-tenant dashboard.

---

### 🎬 Watch the Demo

[![Station Pro Demo](https://github.com/user-attachments/assets/1230b29f-e79c-407c-a877-dbfb29038d19)](https://stationpro.runasp.net/)

> 👆 Click the image to visit the live demo

---

## 📸 Screenshots

### 🔐 Authentication

<img src="https://github.com/user-attachments/assets/d18f8aef-e6c9-4940-8e35-b4c578ef676b" width="48%" alt="Login Page"/>
<img src="https://github.com/user-attachments/assets/25934687-d94b-465c-a58e-f49f51aa8a5b" width="48%" alt="Create Your Store / Register"/>

---

### 💳 Subscription Flow

<img src="https://github.com/user-attachments/assets/b4bb3982-1a2c-48cf-8694-81e41ccf5733" width="48%" alt="Choose Your Plan"/>
<img src="https://github.com/user-attachments/assets/49b52bd5-bbac-4d50-9e0e-5971964c8d57" width="48%" alt="Payment Method & Phone Number"/>

<img src="https://github.com/user-attachments/assets/13d15477-c205-42dc-bf76-f1dbba63d1e6" width="48%" alt="Upload Payment Proof"/>
<img src="https://github.com/user-attachments/assets/32b9772d-a77a-4436-87f3-02e8a8eae57f" width="48%" alt="Pending Review Page"/>

<img src="https://github.com/user-attachments/assets/53f80b84-0b98-4258-aa96-3ade47b22d35" width="48%" alt="Rejection Page"/>

---

### 🔑 Password Reset Flow

<img src="https://github.com/user-attachments/assets/b8a1e1ca-2def-41ac-b1a9-268af04035bd" width="32%" alt="Forgot Password Page"/>
<img src="https://github.com/user-attachments/assets/4b690239-d5c8-4b58-8f51-4811c0bf4403" width="32%" alt="Reset Password Email"/>
<img src="https://github.com/user-attachments/assets/6ff9bd9f-4a5e-4fd2-b4f2-73cdd1fec0cd" width="32%" alt="Reset Password Page"/>

---

### 📊 Tenant Dashboard & Sessions

<img src="https://github.com/user-attachments/assets/af2cf627-2a6a-4f23-b735-97243b32712a" width="100%" alt="Tenant Live Dashboard"/>

<img src="https://github.com/user-attachments/assets/518a0794-50a3-4ebe-a49e-cc9aa042774e" width="100%" alt="Session Management Page"/>

---

### 🖥️ Device & Room Management

<img src="https://github.com/user-attachments/assets/a02bdc2b-943c-413a-9b70-0089d2a5890b" width="48%" alt="Device Management"/>
<img src="https://github.com/user-attachments/assets/00c397f0-2d45-44d0-afec-2aab853d9fc9" width="48%" alt="Room Management"/>

---

### 📈 Reports & Analytics

<img src="https://github.com/user-attachments/assets/13782974-964b-4904-aad1-c08bd7536440" width="48%" alt="Reports Page 1"/>
<img src="https://github.com/user-attachments/assets/0661eb11-be54-41d9-abcd-dd0bb7126099" width="48%" alt="Reports Page 2"/>

<img src="https://github.com/user-attachments/assets/2df980ac-49a7-4894-8fd4-ce8613a23172" width="48%" alt="Print Report"/>
<img src="https://github.com/user-attachments/assets/82c4887d-6283-4193-bd4a-ff3af1ccb0e0" width="48%" alt="Export as Excel / CSV"/>

---

### 🛡️ Admin Panel

<img src="https://github.com/user-attachments/assets/c0a9bc92-8316-4cf9-ba39-5f1320622356" width="100%" alt="Admin Dashboard"/>

<img src="https://github.com/user-attachments/assets/2ab7c5ba-68ae-4c6d-8148-f07cd5464ce5" width="100%" alt="Admin Subscription Requests"/>

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

🌐 **Live Demo:** [stationpro.runasp.net](https://stationpro.runasp.net/)

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
| 📈 **Reports & Analytics** | Detailed reports with PDF print and Excel/CSV export |
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

Hangfire uses a **dedicated, isolated database** separate from the main application database. This keeps Hangfire's internal polling tables from interfering with business data, migrations, or backups.

```
Main DB        → All business entities (Tenants, Devices, Sessions...)
Hangfire DB    → Hangfire job tables only
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
Enters payment method + phone number
    ↓
Uploads payment proof screenshot + transaction reference
    ↓
Request goes to "Pending" state
    ↓
Admin reviews payment proof in admin panel
    ↓
Admin Approves → Tenant is activated, plan is assigned
Admin Rejects  → Tenant sees rejection reason, can resubmit
```

### Subscription Plans

| Plan | Price | Target |
|---|---|---|
| **Basic** | 29 EGP/mo | Small shops — up to 10 devices |
| **Pro** | 79 EGP/mo | Growing businesses — up to 50 devices |
| **Enterprise** | 199 EGP/mo | Large operations — unlimited devices |

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
git clone https://github.com/Mohamed-Khaled970/Station-Pro.git
cd Station-Pro
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

🌐 [stationpro.runasp.net](https://stationpro.runasp.net/)

</div>
