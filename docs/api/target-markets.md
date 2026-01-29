# Target Market Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Target Market Microservice, outlining the industrial sectors served by the ERP system.

**Base URL**: `http://localhost:8082/api/v1/target-markets` (via Gateway)

---

## 🎯 Sector Insights (Public)

### 1. List All Target Markets
Retrieves all market sectors with pagination and search.

**Endpoint**: `GET /`

```bash
curl -X GET "http://localhost:8082/api/v1/target-markets?search=Manufacturing&page=1"
```

### 2. Get Market Details
Retrieves details for a specific market sector.

**Endpoint**: `GET /{id}`

```bash
curl -X GET http://localhost:8082/api/v1/target-markets/1
```

---

## 🛠️ Sector Management (Admin Only)

**Auth Required**: `Bearer <admin_token>`

### 3. Create Target Market
Adds a new industrial sector or market. This endpoint uses `multipart/form-data`.

**Endpoint**: `POST /`

```bash
curl -X POST http://localhost:8082/api/v1/target-markets \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Chemical & Pharmaceutical" \
-F "Description=Specialized ERP modules for batch manufacturing and regulatory compliance." \
-F "ImageFile=@/path/to/sector_icon.jpg" \
-F "SubItems=Batch Processing, FDA Compliance, Raw Material Tracking"
```

### 4. Update Target Market
Updates market content and images. Uses `multipart/form-data`.

**Endpoint**: `PUT /{id}`

```bash
curl -X PUT http://localhost:8082/api/v1/target-markets/1 \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Global Pharma Supply Chain"
```

### 5. Patch Target Market
Partial update for specific fields.

**Endpoint**: `PATCH /{id}`

```bash
curl -X PATCH http://localhost:8082/api/v1/target-markets/1 \
-H "Authorization: Bearer your_admin_token" \
-F "Description=Updated description for Pharma sector."
```

### 6. Delete Target Market
Removes a market sector record.

**Endpoint**: `DELETE /{id}`

```bash
curl -X DELETE http://localhost:8082/api/v1/target-markets/1 \
-H "Authorization: Bearer your_admin_token"
```
