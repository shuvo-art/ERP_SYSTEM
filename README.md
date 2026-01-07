# ERP System Microservices

A modern, enterprise-grade set of microservices built with **.NET 8**, focusing on **Identity Management** and **Product Catalog** services. The project follows **Clean Architecture** patterns and is designed for high performance and scalability.

---

## 🏗️ Architecture Overview

The system is composed of independent services following microservices principles:

- **Auth API**: Handles user registration, verification (OTP via SMTP), JWT-based authentication, and audit logging.
- **Product API**: Manages a complex product catalog including specifications, documents, and related images using a normalized database schema.
- **Shared Kernel**: Common utilities, middleware, and interfaces used across services.

### Core Technologies
- **Framework**: ASP.NET Core 8.0
- **Data Access**: Dapper (Micro-ORM) with SQL Server Stored Procedures
- **Database**: SQL Server 2022
- **Security**: JWT Bearer Auth, BCrypt Password Hashing, Rate Limiting
- **Infrastructure**: Docker & Docker Compose

---

## 🛠️ Project Structure

```text
├── src/
│   ├── Auth.Api              # Identity Entry Point
│   ├── Auth.Core             # Auth Business Logic & Entities
│   ├── Auth.Infrastructure   # DB Repositories & Services Implementation
│   ├── ProductApi.Api        # Product Catalog Entry Point
│   ├── ProductApi.Core       # Product Business Logic
│   ├── ProductApi.Infrastructure # Product Data Access
│   └── Shared.Kernel         # Cross-cutting concerns
├── sql/
│   ├── auth/                 # Auth Database Initialization
│   └── products/             # Products Database Initialization
├── ErpSystem.sln             # Main Solution File
└── docker-compose.yaml       # Container Orchestration
```

---

## � Getting Started

### 1. Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local development)

### 2. Run with Docker Compose
From the root directory, run:

```bash
docker compose up --build -d
```

Validating the services:
- **Auth API Swagger**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Product API Swagger**: [http://localhost:8083/swagger](http://localhost:8083/swagger)
- **Adminer (DB Management)**: [http://localhost:8081](http://localhost:8081)

---

## 🔑 Authentication Flow

1. **Register**: `POST /api/v1/auth/register`
2. **Verify OTP**: Verification code is sent via email (Smtp configurated in docker-compose).
3. **Login**: `POST /api/v1/auth/login` - Returns a JWT access token.
4. **Authorize**: Use the token in the `Authorization: Bearer <token>` header for protected Product API endpoints.

---

## � Product Management

The Product API supports complex objects including:
- **Specification Tables**: Structured JSON data for technical details.
- **Document Links**: TDS, SDS, and Brochures.
- **Image Galleries**: Main image and related previews.

Example Create Product JSON:
```json
{
  "name": "Industrial Coating",
  "category": "Construction",
  "overview": {
    "details": "Technical overview...",
    "specifications": [
      { "title": "Size", "items": ["10L", "20L"] }
    ]
  }
}
```

---

## 🛡️ Security Best Practices
- **Rate Limiting**: Configured per endpoint to prevent brute-force attacks.
- **Auditing**: Every login, registration, and status change is logged in the `AuditLogs` table.
- **Secrets**: Passwords and keys are managed via environment variables and appsettings.
