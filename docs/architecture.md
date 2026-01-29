# Architecture Overview

The ERP System is built using a **Microservices Architecture** pattern, designed for high availability, scalability, and maintainability.

## 🏗️ System Design

The system consists of multiple independent microservices that communicate via HTTP/REST and share common utilities through a Shared Kernel.

### Microservices
- **Auth API**: Identity management, JWT issuance, OTP verification.
- **Product API**: Complex product catalog management.
- **Job API**: Job postings and application management.
- **Target Market API**: Management of target market data.
- **Partner API**: Partner relationship management.
- **Reference Project API**: Portfolio/Reference project showcases.
- **Contact API**: Handling contact inquiries.
- **About Us API**: Managing company information content.

### Shared Kernel
The `Shared.Kernel` project contains common logic used across all services:
- Global Exception Handling
- API Response wrapping
- Common Middlewares
- Utility constants and helpers

## 🧩 Internal Service Architecture (Clean Architecture)

Each microservice follows the **Clean Architecture** principles to ensure separation of concerns:

### 1. Core Layer (`*.Core`)
- **Domain Entities**: Plain C# objects representing the core business data.
- **DTOs (Data Transfer Objects)**: Objects used for data transfer between layers and API responses.
- **Validators**: FluentValidation rules for incoming requests.
- **Interfaces**: Definitions for repositories, services, and external clients.
- **Business Logic**: Core rules that govern the service.

### 2. Infrastructure Layer (`*.Infrastructure`)
- **Data Access**: Implementation of repository interfaces using **Dapper**.
- **Stored Procedures**: All database interactions are executed via SQL Stored Procedures for performance and security.
- **External Services**: Implementation of email services (SMTP), file storage (Cloudinary), etc.

### 3. API Layer (`*.Api`)
- **Controllers**: RESTful endpoints.
- **Configuration**: `appsettings.json`, Program.cs, and dependency injection registration.
- **Middleware**: Service-specific middleware.

## 🗄️ Data Management
- **SQL Server 2022**: Each service has its own schema or database (depending on deployment).
- **Dapper**: A high-performance micro-ORM used for data mapping.
- **Stored Procedures**: Ensures business logic at the data layer is consistent and efficient.

## 🌐 Networking & Gateway
- **Nginx**: Acts as a Reverse Proxy and API Gateway, routing requests to appropriate microservices.
- **JWT Authentication**: All inter-service and client-service communication is secured via JWT tokens.

## 🚀 Infrastructure & DevOps
- **Docker**: Containerization for consistency across environments.
- **Kubernetes (EKS)**: Orchestration for production scale.
- **Terraform**: Infrastructure as Code for provisioning AWS resources.
- **Ansible**: Configuration management for VM-based deployments.
- **Jenkins**: Automation of CI/CD pipelines.
