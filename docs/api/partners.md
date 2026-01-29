# Partner Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Partner Microservice.

**Base URL**: `http://localhost:8082/api/v1/partners` (via Gateway)

---

## 🤝 Partner management

### 1. List Partners
Retrieves a paginated list of company partners.

**Endpoint**: `GET /`

```bash
curl -X GET "http://localhost:8082/api/v1/partners?page=1&limit=10"
```

### 2. Get Partner Details
Retrieves details for a partner using ID or Slug.

**Endpoint**: `GET /{idOrSlug}`

```bash
curl -X GET http://localhost:8082/api/v1/partners/global-logistics-inc
```

### 3. Create Partner (Admin)
Registers a new partner with media assets. Uses `multipart/form-data`.

**Endpoint**: `POST /`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X POST http://localhost:8082/api/v1/partners \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Global Logistics Inc" \
-F "ShortDescription=Leading logistics provider." \
-F "LogoFile=@/path/to/logo.png" \
-F "BuildingImageFile=@/path/to/hq.jpg" \
-F "CompanyProfileJson={\"founded\": \"2005\", \"employees\": \"5000\"}" \
-F "ProductSegmentsJson=[{\"title\": \"Air Freight\", \"description\": \"Fast delivery\"}]"
```

### 4. Delete Partner (Admin)
Removes a partner and their assets from the system.

**Endpoint**: `DELETE /{id}`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X DELETE http://localhost:8082/api/v1/partners/1 \
-H "Authorization: Bearer your_admin_token"
```

---

## 🛠️ Data Structures

### Product Segments JSON
```json
[
  {
    "title": "Segment Name",
    "description": "Short description of the segment"
  }
]
```

### Partner Profile JSON
```json
{
  "key1": "value1",
  "key2": "value2"
}
```
