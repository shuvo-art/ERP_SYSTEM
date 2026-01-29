# API Guide

The ERP System APIs follow a standardized RESTful design pattern.

## 🔑 Authentication

Most endpoints require a valid JWT token.

### Obtaining a Token
1. Register a user: `POST /api/v1/auth/register`
2. Verify via OTP: `POST /api/v1/auth/verify`
3. Login: `POST /api/v1/auth/login`

### Using the Token
Include the token in the `Authorization` header of your requests:
```http
Authorization: Bearer <your_jwt_token>
```

## 📡 Standard Response Format

The system uses a unified response wrapper:

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    "Validation error 1",
    "Validation error 2"
  ]
}
```

## 🚦 HTTP Status Codes
- `200 OK`: Request succeeded.
- `201 Created`: Resource successfully created.
- `400 Bad Request`: Validation errors or invalid input.
- `401 Unauthorized`: Missing or invalid token.
- `403 Forbidden`: Authenticated but lack permissions (Roles).
- `404 Not Found`: Resource does not exist.
- `500 Internal Server Error`: Generic server error.

## 🛠️ Common Endpoints

### Auth Service
- `POST /api/v1/User/register`
- `POST /api/v1/User/login`
- `POST /api/v1/User/{userId}/role`: (Admin only) Assign roles.

### Product Service
- `GET /api/v1/Products`: List all products.
- `GET /api/v1/Products/{id}`: Get details.
- `POST /api/v1/Products`: Create (Staff/Admin).

## 📄 Documentation (Swagger)
Each service provides interactive documentation at `http://localhost:<port>/swagger`. See root `README.md` for the port mapping.
