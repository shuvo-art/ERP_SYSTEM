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
├── src/                      # Microservices Source Code
├── sql/                      # Database Initialization Scripts
├── terraform/                # Infrastructure as Code (AWS EKS)
│   ├── modules/              # Reusable Terraform Modules
│   └── live/                 # Environment configs (Dev, Staging, Prod)
├── ansible/                  # Server Configuration
├── k8s/                      # Helm Charts for K8s
├── scripts/                  # Helper scripts
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

### 📖 API Documentation (Swagger)
- 🔐 **Auth API**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- 📦 **Product API**: [http://localhost:8083/swagger](http://localhost:8083/swagger)
- 🎯 **Target Market API**: [http://localhost:8084/swagger](http://localhost:8084/swagger)
- 🤝 **Partner API**: [http://localhost:8085/swagger](http://localhost:8085/swagger)
- 🏗️ **Reference Project API**: [http://localhost:8086/swagger](http://localhost:8086/swagger)
- 💼 **Job API**: [http://localhost:8087/swagger](http://localhost:8087/swagger)
- 📧 **Contact API**: [http://localhost:8088/swagger](http://localhost:8088/swagger)
- ℹ️ **About Us API**: [http://localhost:8089/swagger](http://localhost:8089/swagger)

### 🛠️ Management & Gateway
- **Adminer (DB Management)**: [http://localhost:8081](http://localhost:8081)
- **API Gateway (Nginx)**: [http://localhost/](http://localhost/)

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

---

## � DevOps & CI/CD

### 1. Environments
- **Development**: Local Docker Compose (`docker-compose.dev.yml`) and `terraform/live/dev`.
- **Staging**: `terraform/live/staging` and Helm-based deployment.
- **Production**: `terraform/live/prod` with full monitoring.

### 2. Monitoring
The system includes a pre-configured monitoring stack:
- **Prometheus**: Metrics collection.
- **Grafana**: Visual dashboards for service health.

### 3. CI/CD Pipeline
Managed via **Jenkins**, handle automatically:
- Docker Image builds and pushes to Registry.
- Infrastructure provisioning via Terraform.
- Service deployment via Helm.
