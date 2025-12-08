# Order Processing API

A RESTful API for processing orders, built with .NET 8, Dapper, and Docker.

## Features

- **Clean Architecture**: Separation of concerns (Api, Core, Infrastructure).
- **RESTful Endpoint**: `POST /api/v1/orders`.
- **SQL Server**: Normalized schema with `Orders`, `OrderItems`, `Customers`.
- **Performance**: High-performance data insertion using Dapper and Stored Procedures.
- **Idempotency**: Handles duplicate requests using a `RequestId` and unique logic.
- **Async Processing**: Simulated non-blocking integration with a Logistics Gateway.
- **Dockerized**: specific `docker-compose` setup for one-click run.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [jq](https://stedolan.github.io/jq/) (Optional, for pretty-printing test output)

## Quick Start (Docker Compose)

The easiest way to run the application is using Docker Compose. This will start the SQL Server, initialize the database schema, and start the API.

1. **Run the application**:
   ```bash
   docker compose up --build
   ```
   Wait for the containers to start. You will see logs indicating that the database is initialized (`order_db_init` exited with code 0).

2. **Verify the API**:
   You can use `curl` to test the endpoint:
   ```bash
   curl -X POST http://localhost:8080/api/v1/orders \
     -H "Content-Type: application/json" \
     -d '{
       "customerId": 1,
       "totalAmount": 100.00,
       "requestId": "550e8400-e29b-41d4-a716-446655440001",
       "items": [{"productId": "P1", "quantity": 1, "unitPrice": 100}]
     }'
   ```

## Structure

```
├── src
│   ├── OrderApi.Api            # Controllers, DTOs, Program.cs
│   ├── OrderApi.Core           # Entities, Interfaces
│   └── OrderApi.Infrastructure # Dapper Repositories, Services
├── sql
│   └── init.sql                # SQL Schema and Stored Procedure
├── docker-compose.yaml         # Orchestration file

```
