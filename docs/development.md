# Development Guide

This guide provides instructions for setting up your local development environment for the ERP System.

## 📋 Prerequisites
- **.NET 8 SDK**
- **Docker Desktop**
- **Visual Studio 2022** or **VS Code**
- **SQL Server Management Studio (SSMS)** or **Azure Data Studio**

## 🚀 Local Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd ERP_SYSTEM
```

> **Note:** Please refer to the [Git Workflow Guide](./git-workflow.md) for detailed instructions on branching strategies and commit conventions.

### 2. Environment Variables
Copy `.env.example` to `.env` and fill in the required values (DB passwords, SMTP settings, Cloudinary keys).
```bash
cp .env.example .env
```

### 3. Run with Docker Compose
To start all services and the database:
```bash
docker compose up -d
```
This will:
- Spin up SQL Server.
- Run database initialization scripts from `/sql`.
- Start all microservices.
- Start Nginx Gateway.

### 4. Database Migrations/Initialization
Database schemas are managed via SQL scripts in the `/sql` directory. When you run `docker compose up`, these scripts are automatically executed to set up tables and stored procedures.

If you make changes to the database:
1. Update/Add scripts in the `sql/` folder.
2. Ensure you add both the table creation and the corresponding stored procedures.

## 🛠️ Developing a Service

### Adding a New Endpoint
1. **Core**: Define the Request/Response DTO and the Validator.
2. **Core**: Define the interface for the Repository call.
3. **Infrastructure**: Create/Update the Stored Procedure in SQL.
4. **Infrastructure**: Implement the Repository method using Dapper to call the SP.
5. **Api**: Create the Controller action and inject the Repository.

### Testing
- Each microservice has a Swagger UI for manual testing.
- See the root `README.md` for Swagger URLs.

### Coding Standards
- Use **Long PascalCase** for File names.
- Use **camelCase** for private fields (`_privateField`).
- Ensure all public methods are documented.
- Use **FluentValidation** for all incoming request DTOs.
- Always use **Async/Await** for I/O operations.

## 📖 Useful Commands
- `docker compose build`: Rebuild images.
- `docker compose logs -f <service-name>`: Tail logs for a specific service.
- `dotnet build`: Build the solution locally.
