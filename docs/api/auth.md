# Auth Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Auth Microservice.

**Base URL**: `http://localhost:8082/api/v1/auth` (via Gateway) or `http://localhost:8080/api/v1/auth` (Direct)

---

## 🔐 Authentication & Identity (Auth Controller)

### 1. Register User
Registers a new user account and sends a verification OTP to the email.

**Endpoint**: `POST /register`

```bash
curl -X POST http://localhost:8082/api/v1/auth/register \
-H "Content-Type: application/json" \
-d '{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}'
```

### 2. Verify Email (OTP)
Verifies the email address using the OTP received after registration.

**Endpoint**: `POST /verify-email`

```bash
curl -X POST http://localhost:8082/api/v1/auth/verify-email \
-H "Content-Type: application/json" \
-d '{
  "email": "user@example.com",
  "otp": "123456"
}'
```

### 3. Login
Authenticates the user and returns a JWT Access Token and Refresh Token.

**Endpoint**: `POST /login`

```bash
curl -X POST http://localhost:8082/api/v1/auth/login \
-H "Content-Type: application/json" \
-d '{
  "email": "user@example.com",
  "password": "Password123!"
}'
```

### 4. Refresh Token
Obtains a new Access Token using a valid Refresh Token.

**Endpoint**: `POST /refresh-token`

```bash
curl -X POST http://localhost:8082/api/v1/auth/refresh-token \
-H "Content-Type: application/json" \
-d '{
  "refreshToken": "your_refresh_token_here"
}'
```

### 5. Forgot Password
Requests a password reset OTP.

**Endpoint**: `POST /forgot-password`

```bash
curl -X POST http://localhost:8082/api/v1/auth/forgot-password \
-H "Content-Type: application/json" \
-d '{
  "email": "user@example.com"
}'
```

### 6. Reset Password
Resets the password using the OTP received via email.

**Endpoint**: `POST /reset-password`

```bash
curl -X POST http://localhost:8082/api/v1/auth/reset-password \
-H "Content-Type: application/json" \
-d '{
  "email": "user@example.com",
  "otp": "123456",
  "newPassword": "NewSecurePassword123!"
}'
```

### 7. Logout
Revokes the refresh token and clears the authentication session.

**Endpoint**: `POST /logout`
**Auth Required**: `Bearer <token>`

```bash
curl -X POST http://localhost:8082/api/v1/auth/logout \
-H "Authorization: Bearer your_access_token" \
-H "Content-Type: application/json" \
-d '{
  "refreshToken": "your_refresh_token_here"
}'
```

---

## 👤 User Management (User Controller)

**Base URL**: `http://localhost:8082/api/v1/users`

### 8. Get Profile
Retrieves the logged-in user's profile information.

**Endpoint**: `GET /profile`
**Auth Required**: `Bearer <token>`

```bash
curl -X GET http://localhost:8082/api/v1/users/profile \
-H "Authorization: Bearer your_access_token"
```

### 9. Update Profile
Updates profile details for the authenticated user.

**Endpoint**: `PUT /profile`
**Auth Required**: `Bearer <token>`

```bash
curl -X PUT http://localhost:8082/api/v1/users/profile \
-H "Authorization: Bearer your_access_token" \
-H "Content-Type: application/json" \
-d '{
  "firstName": "Johnny",
  "lastName": "Doe",
  "phone": "+1234567890",
  "country": "USA",
  "language": "en"
}'
```

### 10. Change Password
Changes the password for the current user.

**Endpoint**: `PUT /change-password`
**Auth Required**: `Bearer <token>`

```bash
curl -X PUT http://localhost:8082/api/v1/users/change-password \
-H "Authorization: Bearer your_access_token" \
-H "Content-Type: application/json" \
-d '{
  "currentPassword": "Password123!",
  "newPassword": "BrandNewPassword456!"
}'
```

---

## 🛠️ Admin Operations (Admin Role Only)

### 11. Get All Users
Lists all active users in the system.

**Endpoint**: `GET /all`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X GET http://localhost:8082/api/v1/users/all \
-H "Authorization: Bearer your_admin_token"
```

### 12. Update User Role
Assigns a new role (e.g., 'Admin' or 'User') to a specific user.

**Endpoint**: `PUT /{userId}/role`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X PUT http://localhost:8082/api/v1/users/123/role \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{
  "role": "Admin"
}'
```

### 13. Delete User
Soft deletes or removes a user from the system.

**Endpoint**: `DELETE /{userId}`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X DELETE http://localhost:8082/api/v1/users/123 \
-H "Authorization: Bearer your_admin_token"
```
