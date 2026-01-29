# Contact Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Contact Microservice, including Distributor management and Enquiries/Support.

**Base URL**: `http://localhost:8082` (via Gateway)

---

## 🏢 Distributors management

**Base URL**: `/api/v1/distributors`

### 1. Get All Distributors
Lists all distributors. Can be filtered for public view.

**Endpoint**: `GET /`

```bash
# Public view (only active)
curl -X GET http://localhost:8082/api/v1/distributors?public=true

# Admin view (all)
curl -X GET http://localhost:8082/api/v1/distributors \
-H "Authorization: Bearer your_admin_token"
```

### 2. Create Distributor
Adds a new distributor.

**Endpoint**: `POST /`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X POST http://localhost:8082/api/v1/distributors \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{
  "name": "Global Supplies Ltd",
  "address": "123 Business Way, New York",
  "phone": "+1-555-0199",
  "country": "USA",
  "email": "contact@globalsupplies.com",
  "website": "https://globalsupplies.com",
  "isActive": true,
  "displayOrder": 1
}'
```

---

## 📧 Enquiries & Support

**Base URL**: `/api/v1/contact/enquiries`

### 3. Submit Enquiry
Public endpoint for visitors to send enquiries or request callbacks.

**Endpoint**: `POST /`

```bash
curl -X POST http://localhost:8082/api/v1/contact/enquiries \
-H "Content-Type: application/json" \
-d '{
  "type": "Product Inquiry",
  "name": "Alice Smith",
  "email": "alice@example.com",
  "mobile": "+123456789",
  "companyName": "TechCorp",
  "message": "I am interested in your ERP solution.",
  "requestCallBack": true,
  "agreeDataProtection": true
}'
```

### 4. Get Enquiries (Admin)
Retrieves a list of submitted enquiries with filtering.

**Endpoint**: `GET /`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X GET "http://localhost:8082/api/v1/contact/enquiries?status=new&type=Product%20Inquiry" \
-H "Authorization: Bearer your_admin_token"
```

### 5. Update Enquiry Status (Admin)
Updates the status of an enquiry (e.g., 'In Progress', 'Resolved').

**Endpoint**: `PATCH /{id}`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X PATCH http://localhost:8082/api/v1/contact/enquiries/550e8400-e29b-41d4-a716-446655440000 \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{
  "status": "Resolved",
  "adminNotes": "Followed up via phone. Customer satisfied."
}'
```
