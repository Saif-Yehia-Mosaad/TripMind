# TripMind Backend — How to Run
## Server: DESKTOP-0MDMFGG\MSSQLSERVER04

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Visual Studio 2022 | 17.8+ | Community edition is free |
| SQL Server | MSSQLSERVER04 | Already installed on your machine |
| SSMS (optional) | any | To inspect the database |

---

## Step 1 — Open the Solution

1. Extract the zip file
2. Double-click **`TripMind.sln`**
3. Visual Studio opens 4 projects:

```
TripMind.sln
 ├── TripMind.Domain          (Class Library)
 ├── TripMind.Application     (Class Library)
 ├── TripMind.Infrastructure  (Class Library)
 └── TripMind.API             (ASP.NET Core Web API)  ← startup project
```

---

## Step 2 — Verify the Connection String

Open `TripMind.API > appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-0MDMFGG\\MSSQLSERVER04;Database=TripMindDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

✅ This uses **Windows Authentication** (`Trusted_Connection=True`).  
✅ No username/password needed — it uses your Windows login.  
✅ The database `TripMindDb` will be **created automatically** in Step 4.

> **If connection fails:** Open SQL Server Configuration Manager → 
> check that `MSSQLSERVER04` is Running and TCP/IP is Enabled.

---

## Step 3 — Restore NuGet Packages

### Option A — Visual Studio (automatic)
Visual Studio restores packages automatically when you open the solution.

### Option B — Command Line
```bash
cd TripMindSolution
dotnet restore
```

---

## Step 4 — Run EF Core Migration (creates the database)

### Option A — Package Manager Console (inside Visual Studio)

1. Go to **Tools → NuGet Package Manager → Package Manager Console**
2. Set **Default project** dropdown to: `TripMind.Infrastructure`
3. Run:

```powershell
Add-Migration InitialCreate -StartupProject TripMind.API
Update-Database -StartupProject TripMind.API
```

### Option B — Terminal / Command Prompt

```bash
cd TripMindSolution

# Install EF tools if not already installed
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate `
  --project TripMind.Infrastructure `
  --startup-project TripMind.API

# Apply migration → creates TripMindDb on MSSQLSERVER04
dotnet ef database update `
  --project TripMind.Infrastructure `
  --startup-project TripMind.API
```

✅ After this step, open SSMS and you will see:
```
DESKTOP-0MDMFGG\MSSQLSERVER04
 └── TripMindDb
      ├── Tables
      │    ├── dbo.Users
      │    ├── dbo.AuditLogs
      │    └── dbo.__EFMigrationsHistory
```

---

## Step 5 — Set Startup Project

Right-click **`TripMind.API`** → **Set as Startup Project**

---

## Step 6 — Run the API

Press **F5** (Debug) or **Ctrl+F5** (without debugger)

The browser opens automatically at:
```
https://localhost:{port}/
```
Which shows the **Swagger UI** — all endpoints are listed and testable.

---

## Step 7 — Test the Endpoints in Swagger

Swagger opens at `https://localhost:{port}/index.html`

### Test Register (Sign Up screen)
```
POST /api/auth/register
{
  "displayName": "Ahmed Hassan",
  "email": "ahmed@test.com",
  "password": "Test@1234",
  "confirmPassword": "Test@1234",
  "rememberMe": false
}
```
Copy the `accessToken` from the response.

### Authorize in Swagger
Click the **🔒 Authorize** button → paste: `Bearer {your_token}`

### Test Login (Sign In screen)
```
POST /api/auth/login
{
  "email": "ahmed@test.com",
  "password": "Test@1234",
  "rememberMe": false
}
```

### Test Forgot Password Flow
```
# Step 1 — send OTP
POST /api/auth/forgot-password
{ "email": "ahmed@test.com" }

# Step 2 — verify OTP (check console output for OTP in dev mode)
POST /api/auth/verify-otp
{ "email": "ahmed@test.com", "otp": "1234" }

# Step 3 — reset password (use resetToken from step 2 response)
POST /api/auth/reset-password
{
  "email": "ahmed@test.com",
  "resetToken": "...",
  "newPassword": "NewPass@5678",
  "confirmNewPassword": "NewPass@5678"
}
```

---

## Project Dependencies (How the layers talk to each other)

```
TripMind.API
  └── references TripMind.Application
  └── references TripMind.Infrastructure

TripMind.Infrastructure
  └── references TripMind.Application
  └── references TripMind.Domain

TripMind.Application
  └── references TripMind.Domain

TripMind.Domain
  └── references nothing  ✅ pure C# entities
```

---

## NuGet Packages Installed

| Package | Project | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | Infrastructure | SQL Server EF provider |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure | Migrations CLI |
| `BCrypt.Net-Next` | Infrastructure | Password hashing (work factor 12) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Infrastructure | JWT validation |
| `System.IdentityModel.Tokens.Jwt` | Infrastructure | JWT generation |
| `Swashbuckle.AspNetCore` | API | Swagger UI |

---

## Common Errors & Fixes

| Error | Fix |
|-------|-----|
| `Cannot open server 'DESKTOP-0MDMFGG\MSSQLSERVER04'` | Open SQL Server Config Manager → Start the MSSQLSERVER04 service |
| `A connection was successfully established... SSL` | Already handled by `TrustServerCertificate=True` |
| `No migrations have been applied` | Run `Update-Database` in Package Manager Console |
| `IJwtProvider not registered` | Check Program.cs has `builder.Services.AddScoped<IJwtProvider, JwtProvider>()` |
| Port already in use | Change port in `TripMind.API > Properties > launchSettings.json` |

---

## Folder Structure Reference

```
TripMind.sln
│
├── TripMind.Domain/                    ← Class Library (no dependencies)
│   └── Entities/
│       ├── User.cs
│       └── AuditLog.cs
│
├── TripMind.Application/               ← Class Library (depends on Domain)
│   ├── DTOs/Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   ├── AuthResponse.cs
│   │   └── ResetPasswordDtos.cs
│   ├── Interfaces/
│   │   └── IJwtProvider.cs
│   └── Services/
│       └── AuthService.cs
│
├── TripMind.Infrastructure/            ← Class Library (depends on Application)
│   ├── Persistence/
│   │   └── TripMindDbContext.cs        ← SQL Server + Fluent API
│   └── Security/
│       ├── JwtProvider.cs             ← JWT token generation
│       └── PasswordHasher.cs          ← BCrypt hashing
│
└── TripMind.API/                       ← ASP.NET Core Web API (startup)
    ├── Controllers/
    │   └── AuthController.cs          ← 5 endpoints
    ├── Middlewares/
    │   └── AuditLogMiddleware.cs      ← auto-logs all /api/auth/* calls
    ├── Program.cs                     ← DI wiring + pipeline
    ├── appsettings.json               ← MSSQLSERVER04 connection string
    └── appsettings.Development.json
```
