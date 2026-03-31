# 🎟️ Enterprise Event Ticketing & Management System API

A production-ready backend API for managing events, ticket sales, payments, and access control — built with .NET 10, Clean Architecture, and Domain-Driven Design.

---

## ✨ Features

- **Event Management** — Create, publish, update, and cancel events with capacity control
- **Ticket Sales** — Order creation, payment processing, expiry handling, and refunds
- **Dual Payment Gateways** — Paystack and Flutterwave with webhook verification
- **Encrypted QR Tickets** — AES-256-GCM encrypted QR codes with PDF delivery via email
- **Access Control** — QR scanning, validation, and anti-fraud checks
- **Finance Ledger** — Double-entry bookkeeping, host payouts, refund transactions
- **Email Notifications** — Transactional emails via Resend (order, ticket, auth, events)
- **Role-Based Auth** — 4 roles: Admin, Host, Customer, Scanner (JWT + Refresh Tokens)
- **Rate Limiting** — Fixed window rate limiting on all endpoints
- **Outbox Pattern** — At-least-once delivery for all domain events with retry + dead-letter

---

## 🏗️ Architecture

```
Modular Monolith + Clean Architecture + DDD + CQRS

├── SharedKernel              (Common base classes, Outbox, interfaces)
├── Identity Module           (Auth, JWT, roles, refresh tokens)
├── Finance Module            (Ledger, payouts, refund transactions)
├── Catalog Module            (Events, ticket types, capacity)
├── Sales Module              (Orders, payments, webhooks, refunds)
├── Fulfillment Module        (QR tickets, PDF generation, email delivery)
└── Access Module             (QR scanning, validation, anti-fraud)
```

Each module follows a strict 4-layer structure:
```
Module.Domain          → Entities, value objects, domain events
Module.Application     → Commands/Queries (CQRS), DTOs, service interfaces
Module.Infrastructure  → EF Core, repositories, external service implementations
Module.Api             → Controllers, dependency injection
```

---

## 🛠️ Tech Stack

| Concern | Technology |
|---------|-----------|
| Language | C# (.NET 10) |
| Framework | ASP.NET Core 10 |
| Database | SQL Server + EF Core 9.0 |
| Authentication | ASP.NET Core Identity + JWT |
| Mediator / CQRS | MediatR |
| Logging | Serilog |
| API Docs | Scalar UI + OpenAPI |
| Payments | Paystack + Flutterwave |
| Email | Resend |
| QR Encryption | AES-256-GCM (QRCoder) |
| PDF Generation | QuestPDF |
| Containerization | Docker + Docker Compose |

---

## 🚀 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommended)
- OR [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) + SQL Server (manual setup)
- A [Resend](https://resend.com) account (free tier works)
- A [Paystack](https://paystack.com) account (test keys work)
- A [Flutterwave](https://flutterwave.com) account (test keys work)

---

### Option 1 — Docker Compose (Recommended)

**1. Clone the repo**
```bash
git clone https://github.com/Hayordeji/EventAndTicketManagementAPI.git
cd EventAndTicketManagementAPI
```

**2. Create your environment file**
```bash
cp .env.example .env.development
```

**3. Fill in your secrets in `.env.development`**
```env
SA_PASSWORD=YourStrongPassword123!
JWT_SECRET=your-super-secret-jwt-key-min-32-chars
QR_ENCRYPTION_KEY=        # Generate: openssl rand -base64 32
RESEND_API_KEY=re_...
PAYSTACK_SECRET_KEY=sk_test_...
FLUTTERWAVE_SECRET_KEY=FLWSECK-...
APP_BASE_URL=http://localhost:8080
```

**4. Copy the example appsettings**
```bash
cp src/Host/TicketingSystem.Api/appsettings.example.json \
   src/Host/TicketingSystem.Api/appsettings.json
```

**5. Start the containers**
```bash
docker compose --env-file .env.development up --build
```


**7. Access the API**

| Resource | URL |
|----------|-----|
| Swager UI (API Docs) | http://localhost:8080/swagger/index.html#/ |

---

### Option 2 — Local .NET SDK (Manual)

**1. Clone and configure**
```bash
git clone https://github.com/Hayordeji/EventAndTicketManagementAPI.git
cd EventAndTicketManagementAPI

cp src/Host/TicketingSystem.Api/appsettings.example.json \
   src/Host/TicketingSystem.Api/appsettings.json
```
Make sure not to commit and push your secrets 

**2. Update connection strings** in `appsettings.json` to point to your local SQL Server

**3. Run migrations**
```bash
# Run from solution root
dotnet ef database update \
  --project src/Modules/Identity/TicketingSystem.Modules.Identity.Infrastructure \
  --startup-project src/Host/TicketingSystem.Api \
  --context IdentityDbContext

# Repeat for each module: Finance, Catalog, Sales, Fulfillment, Access
```

**4. Run the API**
```bash
dotnet run --project src/Host/TicketingSystem.Api
```

---

## 🔑 Generating Secrets

```bash
# JWT Secret (min 32 chars)
openssl rand -base64 32

# QR Encryption Key (must be exactly 32 bytes → 44 Base64 chars)
openssl rand -base64 32

# SQL Server SA Password (must meet SQL Server complexity rules)
openssl rand -base64 24
```

---

## 👤 Roles & Permissions

| Role | Capabilities |
|------|-------------|
| **Admin** | Full access — manage all users, events, and data |
| **Host** | Create and manage their own events, view earnings, request payouts |
| **Customer** | Browse events, purchase tickets, request refunds |
| **Scanner** | Scan and validate QR codes at event entrance |

---

## 📡 API Modules

| Module | Base Path | Key Endpoints |
|--------|-----------|--------------|
| Identity | `/api/auth` | Register, login, refresh, forgot/reset password, email confirmation |
| Catalog | `/api/events` | Create, publish, update, cancel events; manage ticket types |
| Sales | `/api/orders` | Create order, initiate payment, webhook callbacks, refunds |
| Fulfillment | `/api/tickets` | View tickets, download PDF, get QR code |
| Access | `/api/scan` | Validate QR code, view scan logs |
| Finance | `/api/finance` | View ledger, request payout |

Full interactive documentation available at `http://localhost:8080` via Swagger UI.

---

## 🐳 Docker Image

Pre-built images are available on Docker Hub:

```bash
docker pull molefox/ticketing-api:latest
```

Run with your own environment:
```bash
docker run -p 8080:8080 \
  -e "ConnectionStrings__IdentityDb=Server=YOUR_SERVER;Database=TicketingSystem_Identity;..." \
  -e "Jwt__Secret=your-secret" \
  -e "QrEncryption__Key=your-key" \
  YOUR_DOCKERHUB_USERNAME/ticketing-api:latest
```

> **Note:** You still need a running SQL Server instance and must run migrations separately.

---

## 🗄️ Database Structure

Six isolated databases — one per module:

| Module | Database |
|--------|----------|
| Identity | `TicketingSystem_Identity` |
| Finance | `TicketingSystem_Finance` |
| Catalog | `TicketingSystem_Catalog` |
| Sales | `TicketingSystem_Sales` |
| Fulfillment | `TicketingSystem_Fulfillment` |
| Access | `TicketingSystem_Access` |

No cross-module foreign keys. Modules communicate via domain events (Outbox Pattern).

---

## 🔒 Security

- JWT Bearer Token authentication with refresh token rotation
- AES-256-GCM encrypted QR codes (tamper-proof)
- Policy-based authorization (4 roles)
- Webhook signature verification (Paystack + Flutterwave)
- Rate limiting on all endpoints (fixed window, instant 429)
- Non-root Docker container
- Account lockout after 5 failed login attempts (15 min)
- Startup validation — app refuses to start with invalid/missing secrets

---

## 📬 Payment Webhooks

Configure these URLs in your payment gateway dashboards:

| Gateway | Webhook URL |
|---------|------------|
| Paystack | `https://your-domain.com/api/webhooks/paystack` |
| Flutterwave | `https://your-domain.com/api/webhooks/flutterwave` |

---

## 🤝 Contributing

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/your-feature`)
4. Submit a pull request

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgements

Built with [.NET 10](https://dotnet.microsoft.com), [MediatR](https://github.com/jbogard/MediatR), [EF Core](https://docs.microsoft.com/ef/core/), [QuestPDF](https://www.questpdf.com), [QRCoder](https://github.com/codebude/QRCoder), [Resend](https://resend.com), [Scalar](https://scalar.com).